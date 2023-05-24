using Comgo.Application.Common.Interfaces;
using Comgo.Core.Entities;
using Comgo.Core.Enums;
using Comgo.Core.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Transactions.Commands
{
    internal class TransactionHelper
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;

        public TransactionHelper(IAuthService authService, IAppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<Transaction> CreateTransaction(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var reference = $"Comgo_{DateTime.Now.Ticks}";
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    throw new ArgumentException("Transaction creation failed. Invalid user details");
                }

                var entity = new Transaction
                {
                    DebitAddress = request.DebitAddress,
                    CreditAddress = request.CreditAddress,
                    UserId = request.UserId,
                    Amount = request.Amount,
                    TransactionType = request.TransactionType,
                    TransactionReference = string.IsNullOrEmpty(request.Reference) ? reference : request.Reference,
                    PaymentModeType = request.PaymentMode,
                    TransactionStatus = TransactionStatus.Initiated,
                    Narration = request.Description,
                    CreatedDate = DateTime.Now,
                    Status = Status.Active
                };
                await _context.Transactions.AddAsync(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
