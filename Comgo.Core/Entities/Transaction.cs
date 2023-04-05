using Comgo.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class Transaction : GeneralEntity
    {
        public string DebitAddress { get; set; }
        public string CreditAddress { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public string TransactionStatusDesc { get { return TransactionStatus.ToString(); } }
        public PaymentModeType PaymentModeType { get; set; }
        public string PaymentModeTypeDesc { get { return PaymentModeType.ToString(); } }
        public TransactionType TransactionType { get; set; }
        public string TransactionTypeDesc { get { return TransactionType.ToString(); } }
        public string TransactionReference { get; set; }
        public string UserId { get; set; }
    }
}
