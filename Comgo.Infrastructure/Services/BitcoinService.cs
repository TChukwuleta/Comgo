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

        public async Task<(bool success, string message)> CreateDescriptorString(string pubkeyone, string pubkeytwo)
        {
            try
            {
                var userPubkey = new PubKey(pubkeyone);
                if (userPubkey == null)
                {
                    return (false, "Invalid public key one");
                }

                var userPubkeTwo = new PubKey(pubkeytwo);
                if (userPubkey == null)
                {
                    return (false, "Invalid public key two");
                }
                var cosigners = new List<PubKey>
                {
                    userPubkeTwo,
                    userPubkey,
                };
                List<PubKeyProvider> pubKeysProvider = new();
                foreach (var cosigner in cosigners)
                {
                    var provider = PubKeyProvider.NewConst(cosigner);
                    pubKeysProvider.Add(provider);
                }
                var descriptor = OutputDescriptor.NewMulti(2, pubKeysProvider, true, _network);
                var outputDescriptor = OutputDescriptor.NewWSH(descriptor, _network);
                return (true, outputDescriptor.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPairAsync(string userId, string publicKey)
        {
            try
            {
                var existingSignature = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (existingSignature != null)
                {
                    return (true, "User keys already exist", null);
                }
                if (string.IsNullOrEmpty(publicKey))
                {
                    return (false, "Public key must be provided", null);
                }
                var superAdmin = await _authService.GetSuperAdmin(userId);

                var userPubkey = new PubKey(publicKey);
                if (userPubkey == null)
                {
                    return (false, "Invalid public key", null);
                }
                var adminPubkey = new PubKey(_config["AdminPubKey"]);

                var cosigners = new List<PubKey>
                {
                    adminPubkey,
                    userPubkey,
                };
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
                    Status = Status.Active,
                    UserSafeDetails = descriptor.ToString(),
                };
                await _context.Signatures.AddAsync(newSignature);
                await _context.SaveChangesAsync(new CancellationToken());
                var keyPairs = new KeyPairResponse
                {
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

        public async Task<(bool success, string message, decimal amount)> GetDescriptorBalance(string userId)
        {
            try
            {
                var signature = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                var descriptor = OutputDescriptor.Parse(signature.UserSafeDetails, _network);
                if (descriptor == null)
                {
                    return (false, "Invalid user details", 0);
                }
                var outputDescriptor = OutputDescriptor.NewWSH(descriptor, _network);
                var rpc = await CreateRpcClient();
                // Get all unspent transactions
                var outputDescriptors = new ScanTxoutDescriptor(outputDescriptor);
                ScanTxoutSetParameters scanner = new ScanTxoutSetParameters(outputDescriptor);
                var result = rpc.StartScanTxoutSet(scanner);
                if (!result.Success)
                {
                    return (false, "Unable to retrieve unspent output from descriptor", 0);
                }
                return (true, "Descriptor balance retrieved successfully", result.TotalAmount.ToDecimal(MoneyUnit.Satoshi));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> CreatePSBTAsync(string userId, string destinationAddress, decimal amount)
        {
            try
            {
                // Retrieve the output descriptor pertaining that user
                var signature = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                var descriptor = OutputDescriptor.Parse(signature.UserSafeDetails, _network);
                if (descriptor == null)
                {
                    return (false, "Invalid user details");
                }
                var outputDescriptor = OutputDescriptor.NewWSH(descriptor, _network);
                var amountToSend = Money.Satoshis(amount);
                var recipient = BitcoinAddress.Create(destinationAddress, _network);
                if (recipient == null)
                {
                    return (false, "Invalid destination address");
                }
                var desiredOutputs = new[]
                {
                    new TxOut(amountToSend, recipient),
                };
                var generateAddress = await _bitcoinCoreClient.BitcoinRequestServer(walletname, Core.Enums.RPCOperations.deriveaddresses.ToString(), outputDescriptor.ToString());
                var getAddress = JsonConvert.DeserializeObject<GenericListResponse>(generateAddress);
                if (getAddress == null)
                {
                    return (false, "An error occured while retrieving address for descriptor");
                }
                if (!string.IsNullOrEmpty(getAddress.error))
                {
                    return (false, "An error occured while trying to generate change address");
                }
                var changeAddress = BitcoinAddress.Create(getAddress.result.FirstOrDefault(), _network);
                
                var rpc = await CreateRpcClient();
                // Get all unspent transactions
                var outputDescriptors = new ScanTxoutDescriptor(descriptor);
                ScanTxoutSetParameters scanner = new ScanTxoutSetParameters(outputDescriptor);
                var result = await rpc.StartScanTxoutSetAsync(scanner);
                if (!result.Success)
                {
                    return (false, "Unable to retrieve unspent output from descriptor");
                }
                if (result.TotalAmount.Satoshi <= amount)
                {
                    return (false, "Insufficient funds");
                }

                var outt = result.Outputs;
                // coin selection
                Array.Sort(result.Outputs, (a, b) => -a.Coin.Amount.CompareTo(b.Coin.Amount));
                var totalOutputAmount = desiredOutputs.Sum(output => output.Value);
                var selectedCoins = new List<Coin>();
                var totalSelectedAmount = 0L;
                var selectedOutpoints = new List<ScanTxoutOutput>();

                foreach (var coin in result.Outputs)
                {
                    if (totalSelectedAmount > totalOutputAmount)
                        break;
                    selectedOutpoints.Add(coin);
                    selectedCoins.Add(coin.Coin);
                    totalSelectedAmount += coin.Coin.Amount;
                }


                //bitcoin-cli walletcreatefundedpsbt "[{\"txid\":\"myid\",\"vout\":0}]" "[{\"data\":\"00010203\"}]"
                // Inputs
                var inputOption = new List<Dictionary<string, object>>();
                foreach (var input in selectedOutpoints)
                {
                    inputOption.Add(new Dictionary<string, object>
                    {
                        { "txid", input.Coin.Outpoint.Hash.ToString() },
                        { "vout", input.Coin.Outpoint.N }
                    });
                }

                var outputOption = new List<Dictionary<string, object>>();
                foreach (var input in selectedOutpoints)
                {
                    outputOption.Add(new Dictionary<string, object>
                    {
                        //{ "data", "49879816ffbca992d07559d56c0cb8cbc14aa7eb896bc79f532d272595b5906f" }
                        { destinationAddress, amountToSend.ToString() }
                    });
                }

                //var sendErrand = await _bitcoinCoreClient.BitcoinRequestServer(walletname, "walletcreatefundedpsbt", JsonConvert.SerializeObject(inputOption), JsonConvert.SerializeObject(outputOption));


                TxIn[] txIns = new TxIn[result.Outputs.Length];
                for (int i = 0; i < result.Outputs.Length; i++)
                {
                    txIns[i] = new TxIn( new OutPoint(result.Outputs[i].Coin.Outpoint.Hash, result.Outputs[i].Coin.Outpoint.N));
                }
                var outputs = new Dictionary<BitcoinAddress, Money>
                {
                    { recipient, amountToSend }
                };

                Dictionary<string, string> options = new Dictionary<string, string>
                {
                    { "subtractFeeFromOutputs", "1" }
                };

/*
                // Create a list of inputs.
                var inputs = new List<Input>();
                inputs.Add(new Input
                {
                    Txid = "1234567890abcdef",
                    Vout = 0
                });

                // Create a list of outputs.
                var outputs = new List<Output>();
                outputs.Add(new Output
                {
                    Address = "1234567890abcdef1234567890abcdef1234567890abcdef",
                    Amount = 0.1
                });

                // Create a PSBT.*/
                //var psbt = await rpc.WalletCreateFundedPSBTAsync(inputs, outputs);

                var walletTuple = new Tuple<Dictionary<BitcoinAddress, Money>, Dictionary<string, string>>(outputs, options);
                var response = await rpc.WalletCreateFundedPSBTAsync(txIns, walletTuple);

                List<ICoin> iCoinList = new List<ICoin>();
                foreach (Coin coin in selectedCoins)
                {
                    ICoin iCoin = coin; // Convert each Coin to ICoin
                    iCoinList.Add(iCoin);
                }
                // Create a PSBT with the input and output information
                var builder = _network.CreateTransactionBuilder();
                builder.AddCoins(iCoinList); // Convert Coin to ICoin
                builder.Send(recipient, amountToSend);
                builder.SetChange(changeAddress);
                // Set the fee
                var feeRate = new FeeRate(Money.Satoshis(10), 1); // Fee rate in satoshis per byte
                builder.SendEstimatedFees(feeRate);
                var psbt = builder.BuildPSBT(true);
                Console.WriteLine(psbt.ToHex());
                /*//builder.AddKeys(publicKeys);
                var finalize = psbt.Finalize();
                // Finalize the PSBT
                var finalizedTx = psbt.ExtractTransaction();
                // Print the finalized transaction
                Console.WriteLine(finalizedTx.ToHex());
                rpc.SendRawTransaction(finalizedTx);*/
                return (true, psbt.ToHex());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> SignPSBTAsync(string userId, string psbt)
        {
            try
            {
                var rpc = await CreateRpcClient();
                var partialSignedTransaction = PSBT.TryParse(psbt, _network, out PSBT userSignedPSBT);
                var doSth = await rpc.WalletProcessPSBTAsync(userSignedPSBT);
                return (true, "Idan");
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

        private async Task<RPCClient> CreateRpcClientCommand()
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