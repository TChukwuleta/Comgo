﻿using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Response;
using Comgo.Core.Enums;
using Comgo.Infrastructure.Helper;
using Grpc.Core;
using Lnrpc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Infrastructure.Services
{
    public class LightningService : ILightningService
    {
        private readonly IConfiguration _config;
        public LightningService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateInvoice(long satoshis, string message)
        {
            string paymentRequest = default;
            var helper = new LightningHelper(_config);
            try
            {
                var adminInvoice = helper.CreateAdminInvoice(satoshis, message);
                paymentRequest = adminInvoice.PaymentRequest;
                return paymentRequest;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetChannelBalance(UserType userType)
        {
            long response = default;
            var helper = new LightningHelper(_config);
            var channelBalanceRequest = new ChannelBalanceRequest();
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        response = userClient.ChannelBalance(channelBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) }).Balance;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetAdminClient();
                        response = adminClient.ChannelBalance(channelBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) }).Balance;
                        break;
                    default:
                        throw new ArgumentException("Invalid user type specified");
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetWalletBalance(UserType userType)
        {
            var helper = new LightningHelper(_config);
            var walletBalanceRequest = new WalletBalanceRequest();
            long response = default;
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        response = userClient.WalletBalance(walletBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) }).TotalBalance;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetUserClient();
                        response = adminClient.WalletBalance(walletBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) }).TotalBalance;
                        break;
                    default:
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<InvoiceSettlementResponse> ListenForSettledInvoice()
        {
            var settledInvoiceResponse = new InvoiceSettlementResponse();
            try
            {
                var helper = new LightningHelper(_config);
                var txnReq = new InvoiceSubscription();

                var adminClient = helper.GetAdminClient();
                var settledAdminInvoioce = adminClient.SubscribeInvoices(txnReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                using (var call = settledAdminInvoioce)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var invoice = call.ResponseStream.Current;
                        if (invoice.State == Invoice.Types.InvoiceState.Settled)
                        {
                            Console.WriteLine(invoice.ToString());
                            settledInvoiceResponse.PaymentRequest = invoice.PaymentRequest;
                            settledInvoiceResponse.IsKeysend = invoice.IsKeysend;
                            settledInvoiceResponse.Value = invoice.Value;
                            settledInvoiceResponse.Expiry = invoice.Expiry;
                            settledInvoiceResponse.Settled = invoice.Settled;
                            settledInvoiceResponse.SettledDate = invoice.SettleDate;
                            settledInvoiceResponse.SettledIndex = (long)invoice.SettleIndex;
                            settledInvoiceResponse.Private = invoice.Private;
                            settledInvoiceResponse.AmountInSat = invoice.AmtPaidSat;
                            settledInvoiceResponse.Email = invoice.Memo;
                            return settledInvoiceResponse;
                        }
                    }
                }
                return settledInvoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> SendLightning(string paymentRequest, UserType userType)
        {
            string result = default;
            var helper = new LightningHelper(_config);
            var sendRequest = new SendRequest();
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance(userType);
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedPaymentReq = userClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        if (walletBalance < decodedPaymentReq.NumSatoshis)
                        {
                            throw new ArgumentException("Unable to complete lightning payment. Insufficient funds");
                        }
                        sendRequest.Amt = decodedPaymentReq.NumSatoshis;
                        sendRequest.PaymentRequest = paymentRequest;
                        var response = userClient.SendPaymentSync(sendRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        result = response.PaymentError;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetUserClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedAdminPaymentReq = adminClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        if (walletBalance < decodedAdminPaymentReq.NumSatoshis)
                        {
                            throw new ArgumentException("Unable to complete lightning payment. Insufficient funds");
                        }
                        sendRequest.Amt = decodedAdminPaymentReq.NumSatoshis;
                        sendRequest.PaymentRequest = paymentRequest;
                        var adminResponse = adminClient.SendPaymentSync(sendRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        result = adminResponse.PaymentError;
                        break;
                    default:
                        throw new ArgumentException("Invalid user type");
                        break;
                }
                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
