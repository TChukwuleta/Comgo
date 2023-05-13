using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model;
using Comgo.Application.Common.Model.Response;
using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using Comgo.Core.Entities;
using Comgo.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBitcoin.RPC;
using NBitcoin.Scripting;
using NBXplorer;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;
using Newtonsoft.Json;
using QBitNinja.Client;
using System.Net;
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
        private readonly string serverIp;
        private readonly string username;
        private readonly string password;
        private readonly string walletname;
        public BitcoinService(IConfiguration config, IEmailService emailService, IAppDbContext context,
            IEncryptionService encryptionService, IAuthService authService, IBitcoinCoreClient bitcoinCoreClient)
        {
            _config = config;
            _bitcoinCoreClient = bitcoinCoreClient;
            _authService = authService;
            _encryptionService = encryptionService;
            _emailService = emailService;
            _context = context;
            _network = Network.RegTest;
            serverIp = _config["Bitcoin:URL"];
            username = _config["Bitcoin:username"];
            password = _config["Bitcoin:password"];
            walletname = _config["Bitcoin:wallet"];
        }

        public async Task<(bool success, string message)> ConfirmUserTransaction(string userId, string reference)
        {
            try
            {
                var user = await _authService.GetUserById(userId);
                var userSigExist = await _authService.GetSuperAdmin(userId);

                // Check that the public key of the user who wants to make a transfer exist
                // If the user does exist, send OTP to the mail, else throw an error
                var userKey = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (userKey == null)
                {
                    return (false, "No user key record found. Please contact support");
                }
                var generateOtp = await _authService.GenerateOTP(user.user.Email, "confirm-transaction");
                var errorMessage = generateOtp.Message != null ? generateOtp.Message : generateOtp.Messages.FirstOrDefault();
                if (!generateOtp.Succeeded)
                {
                    throw new ArgumentException(errorMessage);
                }
                //var sendEmail = await _emailService.SendEmailMessage(generateOtp.Entity.ToString(), "Transaction confirmation", user.user.Email);
                var sendEmail = await _emailService.SendConfirmationEmailToUser(user.user.Email, user.user.Name, reference, generateOtp.Entity.ToString());
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

        public async Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPairAsync(string userId)
        {
            try
            {
                var existingSignature = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (existingSignature != null)
                {
                    return (true, "User keys already exist", null);
                }
                var superAdmin = await _authService.GetSuperAdmin(userId);
                // Generate key for user
                var userKey = new Key();
                var userPubkey = userKey.PubKey;

                // Generate key for admin
                var adminKey = new Key();
                var adminPubkey = adminKey.PubKey;

                var cosigners = new List<PubKey>
                {
                    adminPubkey,
                    userPubkey,
                };
                // Convert the list of PubKey objects to a list of PubKeyProvider objects
                List<PubKeyProvider> pubKeysProvider = new();
                foreach (var cosigner in cosigners)
                {
                    var provider = PubKeyProvider.NewConst(cosigner);
                    pubKeysProvider.Add(provider);
                }
                // Create an output descriptor object that describes the multisig wallet
                var descriptor = OutputDescriptor.NewMulti(2, pubKeysProvider, true, _network);
                var newSignature = new Signature
                {
                    UserId = userId,
                    UserPubKey = _encryptionService.EncryptData(userPubkey.ToHex()),
                    AdminPubKey = _encryptionService.EncryptData(adminPubkey.ToHex()),
                    CreatedDate = DateTime.Now,
                    Status = Core.Enums.Status.Active,
                    UserSafeDetails = descriptor.ToString(),
                };
                await _context.Signatures.AddAsync(newSignature);
                await _context.SaveChangesAsync(new CancellationToken());
                var keyPairs = new KeyPairResponse
                {
                    PrivateKey = userKey.GetWif(_network).ToString(),
                    PublicKey = userPubkey.ToHex()
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
                var signature = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                var descriptor = OutputDescriptor.Parse(signature.UserSafeDetails, _network);
                if (descriptor == null)
                {
                    return (false, "Invalid user details");
                }
                var outputDescriptor = OutputDescriptor.NewWSH(descriptor, _network);
                var generateAddress = await _bitcoinCoreClient.BitcoinRequestServer(walletname, Core.Enums.RPCOperations.deriveaddresses.ToString(), outputDescriptor.ToString());
                var getAddress = JsonConvert.DeserializeObject<GenericListResponse>(generateAddress);
                if (getAddress == null)
                {
                    return (false, "An error occured while retrieving address for descriptor");
                }
                if (!string.IsNullOrEmpty(getAddress.error))
                {
                    return (false, getAddress.error);
                }
                return (true, getAddress.result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> GenerateAddressAsync(string userId)
        {
            try
            {
                var client = CreateNBXplorerClient(_network);
                var strategy = await GetDerivationStrategy(userId);
                await client.TrackAsync(strategy);
                var address = (await client.GetUnusedAsync(strategy, DerivationFeature.Deposit)).Address;
                return (true, address.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, WalletBalance response)> GetWalletBalance(string userId)
        {
            try
            {
                var client = CreateNBXplorerClient(_network);
                var strategy = await GetDerivationStrategy(userId);
                var userBalance = await client.GetBalanceAsync(strategy);
                var balanceResponse = new WalletBalance
                {
                    Available = userBalance.Available.ToString(),
                    Confirmed = userBalance.Confirmed.ToString(),
                    Unconfirmed = userBalance.Unconfirmed.ToString(),
                    Total = userBalance.Total.ToString(),
                    Immature = userBalance.Immature.ToString()
                };
                return (true, balanceResponse);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<(bool success, string message)> CreatePSBT(string userid, string recipient)
        {
            try
            {
                // Get User required keys
                var userPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userid);
                if (userPair == null)
                {
                    return (false, "An error occured while retrieving user key details. Kindly contact support");
                }
                var safeDetailsDecrypted = _encryptionService.DecryptData(userPair.UserSafeDetails);
                var safeDetails = JsonConvert.DeserializeObject<SaveDetails>(safeDetailsDecrypted);

                var pubKeyBytes = Encoding.ASCII.GetBytes(safeDetails.ExtPubKey);
                var userExtPubKey = new ExtPubKey(pubKeyBytes);

                var keyPath = new KeyPath(safeDetails.KeyPath);
                var userRootKeyPath = new RootedKeyPath(userExtPubKey.ParentFingerprint, keyPath);

                var getExtKey = Encoding.ASCII.GetBytes(safeDetails.ExtKey);
                var userExtKey = ExtKey.CreateFromBytes(getExtKey);


                // Get system required keys
                var systemAdmin = await _authService.GetSuperAdmin(userid);
                var systemPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == systemAdmin.user.UserId);
                if (systemPair == null)
                {
                    return (false, "An error occured while retrieving system key details. Kindly contact support");
                }
                var systemDecryptedSafeDetails = _encryptionService.DecryptData(systemPair.UserSafeDetails);
                var systemSafeDetails = JsonConvert.DeserializeObject<SaveDetails>(systemDecryptedSafeDetails);

                var keyBytes = Encoding.ASCII.GetBytes(systemSafeDetails.ExtPubKey);
                var systemExtPubKey = new ExtPubKey(keyBytes);

                var sysKeyPath = new KeyPath(safeDetails.KeyPath);
                var systemRootKeyPath = new RootedKeyPath(systemExtPubKey.ParentFingerprint, sysKeyPath);

                var getSysExtKey = Encoding.ASCII.GetBytes(systemSafeDetails.ExtKey);
                var systemExtKey = ExtKey.CreateFromBytes(getSysExtKey);


                // Get client, derivation strategy
                var client = CreateNBXplorerClient(_network);
                var strategy = await GetDerivationStrategy(userid);
                var bitcoinAddress = BitcoinAddress.Create(recipient, _network);

                // create psbt
                var psbt = (await client.CreatePSBTAsync(strategy, new CreatePSBTRequest()
                {
                    Destinations =
                    {
                        new CreatePSBTDestination()
                        {
                            Destination = bitcoinAddress,
                            Amount = Money.Coins(0.4m),
                            SubstractFees = true
                        }
                    },
                    FeePreference = new FeePreference()
                    {
                        ExplicitFeeRate = new FeeRate(10.0m)
                    }
                })).PSBT;

                // User signs the psbt
                var userSigns = Sign(userExtPubKey, userRootKeyPath, userExtKey, strategy, psbt);
                // System signs the psbt
                var systemSigns = Sign(systemExtPubKey, systemRootKeyPath, systemExtKey, strategy, psbt);
                // Both signed psbt are combined (2 of 2)
                var signedPSBT = userSigns.Combine(systemSigns);
                signedPSBT.Finalize();
                // Get transactions
                var signedTransaction = signedPSBT.ExtractTransaction();
                // Broadcast the signed transaction to the blockchain
                await client.BroadcastAsync(signedTransaction);
                return (true, "transaction broadcasted successfully");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> ListTransactions(string userid)
        {
            var txResponse = new List<TxOutResponse>();
            try
            {
                var client = CreateNBXplorerClient(_network);
                var strategy = await GetDerivationStrategy(userid);
                var transactionList = await client.GetTransactionsAsync(strategy);
                return (true, JsonConvert.SerializeObject(transactionList));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> SendToAddress(string address, string amountBtc)
        {
            var username = _config["Bitcoin:username"];
            var password = _config["Bitcoin:password"];
            var url = _config["Bitcoin:URL"];
            try
            {
                var destinationAddress = BitcoinAddress.Create(address, _network);
                var credentials = new NetworkCredential { Password = password, UserName = username };
                var rpc = new RPCClient(credentials, url, _network);
                Money.TryParse(amountBtc, out Money value);
                await rpc.SendToAddressAsync(destinationAddress.ScriptPubKey, value);
                return (true, "Money sent successfully");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public Task<(bool success, string message)> TransactionLookOut()
        {
            throw new NotImplementedException();
        }


        
        // Still working on this. Refactoring to fit in NBXplorer
        public async Task<(bool success, string message)> CreateMultisigTransaction(string userid, string recipient)
        {
            var client = new QBitNinjaClient(_network);
            try
            {
                // Get public key for both user and admin
                var superAdmin = await _authService.GetSuperAdmin(userid);
                var systemPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == superAdmin.user.UserId);
                if (systemPair == null)
                {
                    return (false, "An error occured while retrieving system user key details. Kindly contact support");
                }
                var userPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userid);
                if (userPair == null)
                {
                    return (false, "An error occured while retrieving user key details. Kindly contact support");
                }
                var decryptedUserPubKey = _encryptionService.DecryptData(userPair.UserPubKey);
                var decryptedAdminPubKey = _encryptionService.DecryptData(systemPair.UserPubKey);
                /*var userPublicKey = new PubKey(Encoding.ASCII.GetBytes(decryptedUserPubKey));
                var systemPublicKey = new PubKey(Encoding.ASCII.GetBytes(decryptedAdminPubKey));*/
                var userPubKey = new BitcoinExtPubKey(decryptedUserPubKey, _network).ToWif();
                var systemPubKey = new BitcoinExtPubKey(decryptedAdminPubKey, _network).ToWif();

                var userPublicKey = new PubKey(userPubKey);
                var systemPublicKey = new PubKey(systemPubKey);

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
                var signedTransaction = await SignSignature(lucasAddress, sendAmount, coinToSpend, minerFee, changeAddress, userPublicKey, systemPublicKey);

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

        // Going to derive a new method
        public async Task<NBitcoin.Transaction> SignSignature(BitcoinAddress recipientAddress, Money amount, ScriptCoin coin, Money minerFee, BitcoinAddress changeAddress, PubKey userKey, PubKey systemKey)
        {
            var builder = _network.CreateTransactionBuilder();
            try
            {
                var pub = new ExtPubKey("bla");
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

        


        // This method creates client connection to NBXplorer
        private static ExplorerClient CreateNBXplorerClient(Network network)
        {
            NBXplorerNetworkProvider provider = new NBXplorerNetworkProvider(network.ChainName);
            ExplorerClient client = new ExplorerClient(provider.GetFromCryptoCode(network.NetworkSet.CryptoCode));
            return client;
        }

        private static PSBT Sign(ExtPubKey pubkey, RootedKeyPath keypath, ExtKey extKey, DerivationStrategyBase derivationStrategy, PSBT psbt)
        {
            try
            {
                psbt = psbt.Clone();
                psbt.RebaseKeyPaths(pubkey, keypath);
                var spend = psbt.GetBalance(derivationStrategy, pubkey, keypath);
                psbt.SignAll(derivationStrategy, extKey.Derive(keypath), keypath);
                return psbt;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        // Look out for transaction sent to any user's wallet on the system
        private static async Task<NewTransactionEvent> WaitTransaction(LongPollingNotificationSession events, DerivationStrategyBase derivationStrategy)
        {
            while (true)
            {
                var evt = await events.NextEventAsync();
                if (evt is NewTransactionEvent tx)
                {
                    if (tx.DerivationStrategy == derivationStrategy)
                    {
                        return tx;
                    }
                }
            }
        }

        // Handle derivation strategy which is what the system would use to track every user's wallet on the system
        private async Task<DerivationStrategyBase> GetDerivationStrategy(string userid)
        {
            try
            {
                var superAdmin = await _authService.GetSuperAdmin(userid);
                var systemPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == superAdmin.user.UserId);
                if (systemPair == null)
                {
                    throw new ArgumentException("An error occured while retrieving system user key details. Kindly contact support");
                }
                var userPair = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userid);
                if (userPair == null)
                {
                    throw new ArgumentException("An error occured while retrieving user key details. Kindly contact support");
                }
                var decryptedUserPubKey = _encryptionService.DecryptData(userPair.UserPubKey);
                var decryptedAdminPubKey = _encryptionService.DecryptData(systemPair.UserPubKey);

                var userPubKey = new BitcoinExtPubKey(decryptedUserPubKey, _network).ExtPubKey;
                var systemPubKey = new BitcoinExtPubKey(decryptedAdminPubKey, _network).ExtPubKey;

                var factory = new DerivationStrategyFactory(_network);
                var derivationStrategy = factory.CreateMultiSigDerivationStrategy(new[]
                {
                    userPubKey,
                    systemPubKey
                }, 2, new DerivationStrategyOptions() { ScriptPubKeyType = ScriptPubKeyType.SegwitP2SH });
                return derivationStrategy;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<RPCClient> CreateRpcClient()
        {
            try
            {
                var credential = new NetworkCredential
                {
                    UserName = username,
                    Password = password
                };
                var rpc = new RPCClient(credential, $"{serverIp}/wallet/{walletname}", _network);
                return rpc;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}