using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Request
{
    public class TransactionRequest : ITransactionRequestValidator
    {
        public string Description { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
    }
}
