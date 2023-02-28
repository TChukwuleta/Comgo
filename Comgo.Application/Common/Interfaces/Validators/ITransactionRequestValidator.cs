using Comgo.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces.Validators
{
    public interface ITransactionRequestValidator
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
