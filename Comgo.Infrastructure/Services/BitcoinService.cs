using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using Comgo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBitcoin.RPC;
using NBitcoin.Scripting;
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
                return (true, $"An email has been sent to your mail. Kindly confirm by including the OTP: {generateOtp.Entity.ToString()} and reference: {reference}");
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
                Console.WriteLine(JsonConvert.SerializeObject(walletDescriptor));

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
                var decryptedDescriptor = _encryptionService.DecryptData(descriptor);
                var rpc = await CreateRpcClient(_walletname);
                var range = new[] { 0, 1 };

                var deriveAddress = await rpc.SendCommandAsync("deriveaddresses", decryptedDescriptor, range);
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

                var strr = processPSBT.PSBT.ToString();
                var hxx = processPSBT.PSBT.ToHex();
                var b64 = processPSBT.PSBT.ToBase64();

                if (processPSBT.Complete)
                {
                    var completePSBT = await FinalizePSBTAsync(walletname, b64);
                    if (!completePSBT.success)
                    {
                        return (false, "Failed transaction");
                    }
                    return(true, completePSBT.message);
                }
                var psbtResponse = new
                {
                    Value = processPSBT.PSBT.ToString(),
                    HexValue = processPSBT.PSBT.ToHex(),
                    Base64Value = processPSBT.PSBT.ToBase64(),
                    IsComplete = processPSBT.Complete
                };
                return (true, psbtResponse);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, object message)> FinalizePSBTAsync(string walletname, string psbt)
        {
            try
            {
                var rpc = await CreateRpcClient(walletname);
                PSBT.TryParse(psbt, _network, out PSBT request);
                var finalizePSBT = await rpc.SendCommandAsync("finalizepsbt", request);
                var finalePSBT = JsonConvert.DeserializeObject<FInalizePSBTResponse>(finalizePSBT.ResultString);
                var broadcastTxn = await rpc.SendCommandAsync("sendrawtransaction", finalePSBT.hex);
                var response = new
                {
                    TxId = broadcastTxn.ResultString,
                    PSBT = finalePSBT.psbt,
                    HexValue = finalePSBT.hex,
                    Complete = finalePSBT.complete
                };
                return (true, response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<(bool success, string message, decimal amount)> GetDescriptorBalance(string descriptor)
        {
            try
            {
                var decryptedDescriptor = _encryptionService.DecryptData(descriptor);
                var walletDescriptor = OutputDescriptor.Parse(decryptedDescriptor, _network);
                if (walletDescriptor == null)
                {
                    return (false, "Invalid user details", 0);
                }
                var rpc = await CreateRpcClient("");
                // Get all unspent transactions
                var outputDescriptors = new ScanTxoutDescriptor(walletDescriptor);
                ScanTxoutSetParameters scanner = new ScanTxoutSetParameters(walletDescriptor);
                var result = rpc.StartScanTxoutSet(scanner);
                if (!result.Success)
                {
                    return (false, "Unable to retrieve unspent output from descriptor", 0);
                }
                return (true, "Descriptor balance retrieved successfully", result.TotalAmount.ToDecimal(MoneyUnit.BTC));
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