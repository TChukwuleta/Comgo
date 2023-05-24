using Comgo.Application.Common.Interfaces;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

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
        private readonly string _walletname;
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
            _walletname = _config["Bitcoin:wallet"];
        }

        public async Task<(bool success, string message)> CreateUserWallet(User user)
        {
            try
            {
                var rpc = await CreateRpcClient("");
                var userWallet = await rpc.CreateWalletAsync(user.Walletname);
                var userRPC = await CreateRpcClient(user.Walletname);
                var userPoolRefill = await userRPC.SendCommandAsync(NBitcoin.RPC.RPCOperations.keypoolrefill);
                user.IsWalletCreated = true;
                await _authService.UpdateUserAsync(user);
                return (true, "User wallet created successfully");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> ValidateTransactionFromSender(string userId, string reference, string destinationAddress)
        {
            try
            {
                var validateAddress = BitcoinAddress.Create(destinationAddress, _network);
                if (validateAddress == null)
                {
                    return (false, "Invalid bitcoin address specified");
                }
                var user = await _authService.GetUserById(userId);
                var generateOtp = await _authService.GenerateOTP(user.user.Email, "confirm-transaction");
                var errorMessage = generateOtp.Message != null ? generateOtp.Message : generateOtp.Messages.FirstOrDefault();
                if (!generateOtp.Succeeded)
                {
                    throw new ArgumentException(errorMessage);
                }
                /*var sendEmail = await _emailService.SendConfirmationEmailToUser(user.user.Email, user.user.Name, reference, generateOtp.Entity.ToString());
                if (!sendEmail)
                {
                    throw new ArgumentException("An error occured while trying to confirm your transaction");
                }*/
                return (true, "An email has been sent to your mail. Kindly confirm by including the OTP");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> GenerateDescriptor(User user)
        {
            try
            {
                var rpc = await CreateRpcClient(user.Walletname);
                var walletAddress = await rpc.GetNewAddressAsync();
                var addressInfo = await rpc.GetAddressInfoAsync(walletAddress);
                var publicKey = addressInfo.PubKey.ToString();
                user.PublicKey = _encryptionService.EncryptData(publicKey);
                await _authService.UpdateUserAsync(user);
                return (true, addressInfo.Descriptor.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> ImportDescriptor(User user)
        {
            var adminDescriptor = _config["Bitcoin:AdminDescriptor"];
            try
            {
                var userRpc = await CreateRpcClient(user.Walletname);
                var descriptorResponse = await userRpc.SendCommandAsync("listdescriptors");
                var userDescriptor = JsonConvert.DeserializeObject<ListDescriptorsResponse>(descriptorResponse.ResultString).descriptors.FirstOrDefault()?.desc;
                string[] partsOne = userDescriptor.Split('[');
                string strippedUserDescriptor = partsOne[1].Split(')')[0];
                string descriptor = $"wsh(sortedmulti(2,{adminDescriptor},[{strippedUserDescriptor}))";
                var getdescriptorInfo = await userRpc.SendCommandAsync("getdescriptorinfo", descriptor);
                var descriptorInfo = JsonConvert.DeserializeObject<DescriptorInfoResponse>(getdescriptorInfo.ResultString);
                descriptor = $"{descriptor}#{descriptorInfo.checksum}";
                user.Descriptor = _encryptionService.EncryptData(descriptor);
                await _authService.UpdateUserAsync(user);
                var rpc = await CreateRpcClient(_walletname);
                var walletDescriptor = new Dictionary<string, object>
                {
                    {"desc", descriptor },
                    {"timestamp", "now" },
                    {"internal", true },
                    {"active", true },
                    {"range",  new List<int> { 0, 100 } }
                };

                var response = await _bitcoinCoreClient.BitcoinRequestServer(_walletname, Core.Enums.RPCOperations.importdescriptors.ToString(), new List<object> { walletDescriptor });
                if (string.IsNullOrEmpty(response))
                {
                    return (false, $"Failed to import descriptor.");
                }
                return (true, descriptor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> GenerateDescriptorAddress(string descriptor)
        {
            try
            {
                var rpc = await CreateRpcClient(_walletname);
                var range = new[] { 0, 1 };

                var deriveAddress = await rpc.SendCommandAsync("deriveaddresses", descriptor, range);
                var descriptorAddresses = JsonConvert.DeserializeObject<List<string>>(deriveAddress.ResultString);
                return (true, descriptorAddresses.FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        public async Task<(bool success, PSBTResponse message)> CreateWalletPSTAsync(decimal amount, string address)
        {
            try
            {
                var rpc = await CreateRpcClient(_walletname);
                var btcAddress = BitcoinAddress.Create(address, _network);

                TxIn[] inputs = new TxIn[] { };
                var outputs = new Dictionary<BitcoinAddress, string>()
                {
                    { btcAddress, amount.ToString() }
                };
                var options = new Dictionary<string, string>()
                {
                    { "fee_rate", "20" }
                };
                var param = Tuple.Create(outputs, options);

                var psbtResponse = await rpc.SendCommandAsync("walletcreatefundedpsbt", JArray.FromObject(inputs), JToken.FromObject(outputs), 0, JObject.FromObject(options));
                var psbt = JsonConvert.DeserializeObject<PSBTResponse>(psbtResponse.ResultString);
                return (true, psbt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, object message)> ProcessPSBTAsync(string walletname, string psbt)
        {
            try
            {
                var rpc = await CreateRpcClient(walletname);
                PSBT.TryParse(psbt, _network, out PSBT request);
                var processPSBT = await rpc.WalletProcessPSBTAsync(request);
                var psbtResponse = new
                {
                    PSBT = processPSBT.PSBT.ToBase64(),
                    Complete = processPSBT.Complete
                };
                Console.WriteLine(JsonConvert.SerializeObject(processPSBT.PSBT));
                return (true, psbtResponse);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> FinalizePSBTAsync(string walletname, string psbt)
        {
            try
            {
                var rpc = await CreateRpcClient(walletname);
                PSBT.TryParse(psbt, _network, out PSBT request);
                var finalizePSBT = await rpc.SendCommandAsync("finalizepsbt", request);
                return (true, finalizePSBT.ResultString);
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
                var generateAddress = await _bitcoinCoreClient.BitcoinRequestServer("", Core.Enums.RPCOperations.deriveaddresses.ToString(), outputDescriptor.ToString());
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
                var rpc = await CreateRpcClient("");
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
                var selectedCoins = new List<Coin>();
                var totalSelectedAmount = 0L;
                var selectedOutpoints = new List<ScanTxoutOutput>();
                var transaction = _network.CreateTransaction();
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
                var generateAddress = await _bitcoinCoreClient.BitcoinRequestServer("", Core.Enums.RPCOperations.deriveaddresses.ToString(), outputDescriptor.ToString());
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
                
                var rpc = await CreateRpcClient("");
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

                foreach (var coin in result.Outputs)
                {
                    if (totalSelectedAmount > totalOutputAmount)
                        break;
                    selectedOutpoints.Add(coin);
                    selectedCoins.Add(coin.Coin);
                    totalSelectedAmount += coin.Coin.Amount;
                }
                // Inputs
                var inputOption = new List<Dictionary<string, object>>();
                foreach (var input in selectedOutpoints)
                {
                    transaction.Inputs.Add(new TxIn(new OutPoint(input.Coin.Outpoint.Hash, input.Coin.Outpoint.N)));
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


                var walletTuple = new Tuple<Dictionary<BitcoinAddress, Money>, Dictionary<string, string>>(outputs, options);
                //var response = await rpc.WalletCreateFundedPSBTAsync(txIns, walletTuple);

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

                transaction.Outputs.Add(new TxOut(amountToSend, recipient));
                var change = Money.Satoshis(150);
                transaction.Outputs.Add(new TxOut(change, changeAddress));
                var psbt = builder.CreatePSBTFrom(transaction, false);

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
                var rpc = await CreateRpcClient("");
                rpc.CreateWallet(userId);
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


        private async Task<RPCClient> CreateRpcClient(string walletname)
        {
            try
            {
                var credential = new NetworkCredential
                {
                    UserName = username,
                    Password = password
                };
                RPCClient rpc = default;
                if (string.IsNullOrEmpty(walletname))
                {
                    rpc = new RPCClient(credential, $"{serverIp}", _network);
                }
                else
                {
                    rpc = new RPCClient(credential, $"{serverIp}/wallet/{walletname}", _network);
                }
                return rpc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}