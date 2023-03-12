using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using Comgo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBXplorer.DerivationStrategy;
using Newtonsoft.Json;
using QBitNinja.Client;
using System;
using System.IO;

namespace Comgo.Infrastructure.Services
{
    public class BitcoinService : IBitcoinService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IAppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuthService _authService;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        private readonly Network _network;
        public BitcoinService(IConfiguration config, IEmailService emailService, IAppDbContext context, 
            IEncryptionService encryptionService, IAuthService authService, IBitcoinCoreClient bitcoinCoreClient)
        {
            _config = config;
            _authService = authService;
            _bitcoinCoreClient = bitcoinCoreClient;
            _encryptionService = encryptionService;
            _emailService = emailService;
            _context = context;
            _network = Network.RegTest;
        }

        public async Task<(bool success, string message)> ConfirmUserTransaction(string userId, string email)
        {
            try
            {
                var user = await _authService.GetUserById(userId);
                var userSigExist = await _authService.GetSuperAdmin(userId);
                if (string.IsNullOrEmpty(userSigExist.user.Key))
                {
                    var userCustody = await _context.UserCustodies.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (userCustody == null)
                    {
                        var newKey = new Key();
                        var serializedKey = JsonConvert.SerializeObject(newKey);
                        var encryptedKey = _encryptionService.EncryptData(serializedKey);
                        var newCustody = new UserCustody
                        {
                            CreatedDate = DateTime.Now,
                            Status = Core.Enums.Status.Active,
                            UserId = userId,
                            Key = encryptedKey
                        };
                        await _context.UserCustodies.AddAsync(newCustody);
                        await _context.SaveChangesAsync(new CancellationToken());
                    }
                }
                var generateOtp = await _authService.GenerateOTP(email, "confirm-transaction");
                var errorMessage = generateOtp.Message != null ? generateOtp.Message : generateOtp.Messages.FirstOrDefault();
                if (!generateOtp.Succeeded)
                {
                    throw new ArgumentException(errorMessage);
                }
                var sendEmail = await _emailService.SendEmailMessage(generateOtp.Entity.ToString(), "Transaction confirmation", user.user.Email);
                if (!sendEmail)
                {
                    throw new ArgumentException("An error occured while trying to confirm your transaction");
                }
                return (true, "An email has been sent to your mail. Kindly confirm by including the OTP");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<(bool success, string message)> GenerateAddress(string userId)
        {
            try
            {
                var user = await _authService.GetUserById(userId);
                var adminUser = await _authService.GetSuperAdmin(userId);
                if (string.IsNullOrEmpty(adminUser.user.Key))
                {
                    return (false, "An error occured while trying to generate address. Please contact admin");
                }

                var decryptedUserKey = _encryptionService.DecryptData(user.user.Key);
                var decryptedAdminKey = _encryptionService.DecryptData(adminUser.user.Key);

                var userKey = JsonConvert.DeserializeObject<Key>(decryptedUserKey);
                var adminKey = JsonConvert.DeserializeObject<Key>(decryptedAdminKey);

                var userKeySecret = userKey.GetBitcoinSecret(_network);
                var adminKeySecret = adminKey.GetBitcoinSecret(_network);

                // Generate p2sh 2 of 2 multisig address
                var redeemScript = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(2, new[] { userKeySecret.PubKey, adminKeySecret.PubKey })
                    .PaymentScript;

                var address = redeemScript.Hash.GetAddress(_network);
                return (true, JsonConvert.SerializeObject(address));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> CreateMultisigTransaction(string userid, string recipient)
        {
            try
            {
                var adminUser = await _authService.GetSuperAdmin(userid);
                var user = await _authService.GetUserById(userid);

                var decryptedUserKey = _encryptionService.DecryptData(user.user.Key);
                if (string.IsNullOrEmpty(decryptedUserKey))
                {
                    return "failed";
                }

                var decryptedAdminKey = _encryptionService.DecryptData(adminUser.user.Key);
                if (string.IsNullOrEmpty(decryptedAdminKey))
                {
                    return "failed";
                }

                var userPublicKey = JsonConvert.DeserializeObject<PubKey>(decryptedUserKey);
                var adminPublicKey = JsonConvert.DeserializeObject<PubKey>(decryptedAdminKey);

                var scriptPubKey = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(2, new[] { userPublicKey, adminPublicKey });

                Console.Write("Public script: " + scriptPubKey);

                var redeemScript = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(2, new[] { userPublicKey, adminPublicKey })
                    .PaymentScript;

                Console.Write("Redeem script: " + redeemScript);

                /*var address = redeemScript.Hash.GetAddress(network);
                return JsonConvert.SerializeObject(address);*/
                var client = new QBitNinjaClient(_network);
                var receivedTransactionId = uint256.Parse("0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a");
                var receiveTransactionResponse = client.GetTransaction(receivedTransactionId).Result;

                // Select which output of transaction should be spent
                var receivedCoins = receiveTransactionResponse.ReceivedCoins;
                OutPoint outpointToSpend = null;
                ScriptCoin coinToSpend = null;
                foreach (var c in receivedCoins)
                {
                    try
                    {
                        coinToSpend = new ScriptCoin(c, redeemScript);
                        outpointToSpend = c.Outpoint;
                    }
                    catch
                    {
                    }
                }

                if (outpointToSpend == null)
                {
                    throw new Exception("Txout doesnt contain any our scriptpubkey");
                }

                Console.WriteLine("We want to spend outpoint #{0}", outpointToSpend.N + 1);

                var userKey = new Key();
                var userKeyWif = userKey.GetBitcoinSecret(_network);

                var adminKey = new Key();
                var adminKeyWif = adminKey.GetBitcoinSecret(_network);

                var recipientAddress = BitcoinAddress.Create(recipient, _network);
                TransactionBuilder builder = _network.CreateTransactionBuilder();
                var minerFee = new Money(0.0002m, MoneyUnit.BTC);
                var txInAmount = (Money)receivedCoins[(int)outpointToSpend.N].Amount;
                var sendAmount = txInAmount - minerFee;

                NBitcoin.Transaction unsigned = builder
                    .AddCoins(coinToSpend)
                    .Send(recipientAddress, sendAmount)
                    .SendFees(minerFee)
                    .SetChange(redeemScript.Hash.GetAddress(_network))
                    .BuildTransaction(sign: false);

                NBitcoin.Transaction userSigned = builder
                    .AddCoins(coinToSpend)
                    .AddKeys(userKeyWif)
                    .SignTransaction(unsigned);

                NBitcoin.Transaction adminSigns = builder
                    .AddCoins(coinToSpend)
                    .AddKeys(adminKeyWif)
                    .SignTransaction(userSigned);

                NBitcoin.Transaction fullySigned = builder
                    .AddCoins(coinToSpend)
                    .CombineSignatures(userSigned, adminSigns);

                Console.WriteLine(fullySigned);

                var broadcastResponse = client.Broadcast(fullySigned).Result;
                if (!broadcastResponse.Success)
                {
                    Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                    Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
                }
                else
                {
                    Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                    Console.WriteLine(fullySigned.GetHash());
                }

                return "done";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<(bool success, string message)> CreateNewKeyPair(string userId)
        {
            try
            {
                var userKey = new Key();
                var userKeySecret = userKey.GetBitcoinSecret(_network).ToWif();
                var encryptedUserKey = _encryptionService.EncryptData(userKeySecret);
                var user = await _authService.GetUserById(userId);
                user.user.Key = encryptedUserKey;
                var updatedUser = await _authService.UpdateUserAsync(user.user);
                if (!updatedUser.Succeeded)
                {
                    return (false, "An error occured. Please try again later");
                }
                var adminKey = new Key();
                var adminKeySecret = adminKey.GetBitcoinSecret(_network).ToWif();
                var encryptedAdminKey =  _encryptionService.EncryptData(adminKeySecret);
                var newCustody = new UserCustody
                {
                    UserId = userId,
                    Key = encryptedAdminKey,
                    CreatedDate = DateTime.Now,
                    Status = Core.Enums.Status.Active
                };
                await _context.UserCustodies.AddAsync(newCustody);
                await _context.SaveChangesAsync(new CancellationToken());
                return (true, "User custody created successfully");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}


/*Console.WriteLine("Enter private key:");
string private_key = Console.ReadLine();
var bitcoinPrivateKey = new BitcoinSecret(private_key, Network.TestNet);

var legacy_address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy);
var segwitp2sh_address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.SegwitP2SH);
var nativesegwit_address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Segwit);

Console.WriteLine("Private Key :" + bitcoinPrivateKey);
Console.WriteLine("Legacy Address :" + legacy_address);
Console.WriteLine("Segwit-P2SH Address :" + segwitp2sh_address);
Console.WriteLine("Bech32 Address :" + nativesegwit_address);*/