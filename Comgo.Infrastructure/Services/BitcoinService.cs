using Comgo.Application.Common.Interfaces;
using Comgo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Infrastructure.Services
{
    public class BitcoinService : IBitcoinService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IAppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuthService _authService;
        public BitcoinService(IConfiguration config, IEmailService emailService, IAppDbContext context, IEncryptionService encryptionService, IAuthService authService)
        {
            _config = config;
            _authService = authService;
            _encryptionService = encryptionService;
            _emailService = emailService;
            _context = context;
        }

        public async Task<string> ConfirmUserTransaction(string privKey, string userId, string email)
        {
            try
            {
                var userSignature = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (userSignature == null)
                {
                    throw new ArgumentException("No signature record found for this user");
                }
                var encryptUserKey = _encryptionService.EncryptData(privKey);
                userSignature.UserKey = encryptUserKey;
                _context.Signatures.Update(userSignature);
                await _context.SaveChangesAsync(new CancellationToken());
                var generateOtp = await _authService.GenerateOTP(email);
                var errorMessage = generateOtp.Message != null ? generateOtp.Message : generateOtp.Messages.FirstOrDefault();
                if (!generateOtp.Succeeded)
                {
                    throw new ArgumentException(errorMessage);
                }
                var sendEmail = await _emailService.SendEmailMessage(generateOtp.Entity.ToString(), "Transaction confirmation");
                if (!sendEmail)
                {
                    throw new ArgumentException("An error occured while trying to confirm your transaction");
                }
                return "An email has been sent to your mail. Kindly confirm by including the OTP";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> GenerateAddress(string privkey, string userId)
        {
            var network = Network.RegTest;
            try
            {
                // Get system encrpted key regarding this particular user
                var systemRecord = await _context.Signatures.FirstOrDefaultAsync(c => c.UserId == userId);
                if (systemRecord == null)
                {
                    systemRecord = await this.GenerateSystemKey(userId);
                }
                var userPubKey = new BitcoinSecret(privkey, network).PubKey;

                /*var legacy_address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy);
                var segwitp2sh_address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.SegwitP2SH);
                var nativesegwit_address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Segwit);*/


                // Decrypt system key record for that particular user;
                var decryptedAdminKey = _encryptionService.DecryptData(systemRecord.SystemKey);
                var adminPubKey = new BitcoinSecret(decryptedAdminKey, network).PubKey;


                // Generate p2sh 2 of 2 multisig address
                var redeemScript = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(2, new[] { userPubKey, adminPubKey })
                    .PaymentScript;

                var address = redeemScript.Hash.GetAddress(network);
                return JsonConvert.SerializeObject(address);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Signature> GenerateSystemKey(string userId)
        {
            var network = Network.RegTest;
            try
            {
                var superAdmin = await _authService.GetSuperAdmin();
                if (superAdmin.user == null)
                {
                    throw new ArgumentException("An error occured while retrieving super admin details");
                }
                var key = new Key();
                var adminKey = key.GetWif(network).ToString();
                var encryptedAdminKey = _encryptionService.EncryptData(adminKey);
                var signature = new Signature
                {
                    AdminUserId = superAdmin.user.UserId,
                    SystemKey = encryptedAdminKey,
                    UserId = userId
                };
                await _context.Signatures.AddAsync(signature);
                await _context.SaveChangesAsync(new CancellationToken());
                return signature;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
