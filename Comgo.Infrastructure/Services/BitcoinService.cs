using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBXplorer;
using NBXplorer.DerivationStrategy;
using Newtonsoft.Json;
using QBitNinja.Client;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

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
        private readonly ChainName _chainNetwork;
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
            _chainNetwork = ChainName.Regtest;
        }

        public async Task<(bool success, string message)> ConfirmUserTransaction(string userId, string email)
        {
            try
            {
                var user = await _authService.GetUserById(userId);
                var userSigExist = await _authService.GetSuperAdmin(userId);

                var userKey = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (userKey == null)
                {
                    return (false, "No user key record found. Please contact support");
                }

                var generateOtp = await _authService.GenerateOTP(email, "confirm-transaction");
                var errorMessage = generateOtp.Message != null ? generateOtp.Message : generateOtp.Messages.FirstOrDefault();
                if (!generateOtp.Succeeded)
                {
                    throw new ArgumentException(errorMessage);
                }
                //var sendEmail = await _emailService.SendEmailMessage(generateOtp.Entity.ToString(), "Transaction confirmation", user.user.Email);
                var sendEmail = await _emailService.SendRegistrationEmailToUser(email, generateOtp.Entity.ToString());
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


        public async Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPair(string userId)
        {
            try
            {
                var user = await _authService.GetUserById(userId);
                var superAdmin = await _authService.GetSuperAdmin(userId);

                var userKey = new Key();
                var userKeySecret = userKey.GetBitcoinSecret(_network).ToWif();
                var userPubKey = userKey.PubKey.ToBytes();
                var userPublicKey = Encoding.UTF8.GetString(userPubKey);

                var systemKey = new Key();
                var systemKeySecret = systemKey.GetBitcoinSecret(_network).ToWif();
                var systemPubKey = systemKey.PubKey.ToBytes();
                var systemPublicKey = Encoding.UTF8.GetString(systemPubKey);


                var existingSignature = superAdmin.user.Signatures.FirstOrDefault(c => c.UserId == userId);
                if (existingSignature == null)
                {
                    existingSignature.AdminUserId = superAdmin.user.UserId;
                    existingSignature.SystemKey = _encryptionService.EncryptData(systemKeySecret);
                    existingSignature.SystemPubKey = _encryptionService.EncryptData(systemPublicKey);
                    existingSignature.UserPubKey = _encryptionService.EncryptData(userPublicKey);
                    existingSignature.CreatedDate = DateTime.Now;
                    existingSignature.Status = Core.Enums.Status.Active;
                    existingSignature.UserId = userId;
                    await _context.Signatures.AddAsync(existingSignature);
                }
                else
                {
                    existingSignature.AdminUserId = superAdmin.user.UserId;
                    existingSignature.SystemKey = _encryptionService.EncryptData(systemKeySecret);
                    existingSignature.SystemPubKey = _encryptionService.EncryptData(systemPublicKey);
                    existingSignature.UserPubKey = _encryptionService.EncryptData(userPublicKey);
                    _context.Signatures.Update(existingSignature);
                }
                await _context.SaveChangesAsync(new CancellationToken());

                var keyPairs = new KeyPairResponse
                {
                    PublicKey = userPublicKey,
                    PrivateKey = userKeySecret
                };
                return (true, "User keys created successfully", keyPairs);
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
                var userPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (userPair == null)
                {
                    return (false, "An error occured while retrieving user key details. Kindly contact support");
                }
                var userPubKey = _encryptionService.DecryptData(userPair.UserPubKey);
                var systemPubKey = _encryptionService.DecryptData(userPair.SystemPubKey);

                var userPublicKey = new PubKey(Encoding.ASCII.GetBytes(userPubKey));
                var adminPublicKey = new PubKey(Encoding.ASCII.GetBytes(systemPubKey));
                // Generate p2sh 2 of 2 multisig address
                var redeemScript = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(2, new[] { userPublicKey, adminPublicKey })
                    .PaymentScript;

                var address = redeemScript.Hash.GetAddress(_network);
                return (true, JsonConvert.SerializeObject(address.ToString()));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<(bool success, string message)> CreateMultisigTransaction(string userid, string recipient)
        {
            var client = new QBitNinjaClient(_network);
            try
            {
                // Get public key for both user and admin
                var userPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userid);
                if (userPair == null)
                {
                    return (false, "An error occured while retrieving user key details. Kindly contact support");
                }
                var decryptedUserPubKey = _encryptionService.DecryptData(userPair.UserPubKey);
                var decryptedAdminPubKey = _encryptionService.DecryptData(userPair.SystemPubKey);
                var userPublicKey = new PubKey(Encoding.ASCII.GetBytes(decryptedUserPubKey));
                var systemPublicKey = new PubKey(Encoding.ASCII.GetBytes(decryptedAdminPubKey));

                // Get the redeem script and addresses
                var scriptPubKey = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(2, new[] { userPublicKey, systemPublicKey });
                Console.Write("Public script: " + scriptPubKey);
                Console.Write("Redeem script: " + scriptPubKey.PaymentScript);

                var lucasAddress = BitcoinAddress.Create(recipient, _network);
                var changeAddress = scriptPubKey.PaymentScript.Hash.GetAddress(_network);


                var receiveTransactionId = uint256.Parse("0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a");
                var receiveTransactionResponse = client.GetTransaction(receiveTransactionId).Result;
                Console.WriteLine(receiveTransactionResponse.TransactionId);
                // if this fails, it's ok. It hasn't been confirmed in a block yet. Proceed
                Console.WriteLine(receiveTransactionResponse.Block.Confirmations);
                var receivedCoins = receiveTransactionResponse.ReceivedCoins;
                OutPoint outpointToSpend = null;
                ScriptCoin coinToSpend = null;
                foreach (var c in receivedCoins)
                {
                    try
                    {
                        // If we can make a ScriptCoin out of our redeemScript
                        // we "own" this outpoint
                        coinToSpend = new ScriptCoin(c, scriptPubKey.PaymentScript);
                        outpointToSpend = c.Outpoint;
                    }
                    catch { }
                }
                if (outpointToSpend == null)
                    throw new Exception("TxOut doesn't contain any our ScriptPubKey");
                Console.WriteLine("We want to spend outpoint #{0}", outpointToSpend.N + 1);
                var minerFee = new Money(0.0002m, MoneyUnit.BTC);
                var txInAmount = (Money)receivedCoins[(int)outpointToSpend.N].Amount;
                var sendAmount = txInAmount - minerFee;

                // Sign transactions with pub key
               var signedTransaction =  await SignSignature(lucasAddress, sendAmount, coinToSpend, minerFee, changeAddress, userPublicKey, systemPublicKey);

                // Broadcast signed transactions
                var broadcastResponse = client.Broadcast(signedTransaction).Result;
                if (!broadcastResponse.Success)
                {
                    throw new ArgumentException($"Threw error with code: {broadcastResponse.Error.ErrorCode}, and message: {broadcastResponse.Error.Reason}");
                }
                return (true, $"Success! You can check out the hash of the transaciton in any block explorer: {signedTransaction.GetHash().ToString()}");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Transaction> SignSignature(BitcoinAddress recipientAddress, Money amount, ScriptCoin coin, Money minerFee, BitcoinAddress changeAddress, PubKey userKey, PubKey systemKey)
        {
            var builder = _network.CreateTransactionBuilder();
            try
            {
                var unsignedTransaction = builder
                    .AddCoin(coin)
                    .Send(recipientAddress, amount)
                    .SendFees(minerFee)
                    .SetChange(changeAddress)
                    .BuildTransaction(sign: false);

                var systemSignature = builder
                    .AddCoin(coin)
                    .AddKeys((ISecret)systemKey)
                    .SignTransaction(unsignedTransaction);

                var userSignature = builder
                    .AddCoin(coin)
                    .AddKeys((ISecret)userKey)
                    .SignTransaction(systemSignature);

                var fullySignedTransaction = builder
                    .AddCoins(coin)
                    .CombineSignatures(systemSignature, userSignature);

                Console.WriteLine(fullySignedTransaction);
                if (fullySignedTransaction == null)
                {
                    throw new ArgumentException("An error occured while trying to sign transactions.");
                }
                return fullySignedTransaction;
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