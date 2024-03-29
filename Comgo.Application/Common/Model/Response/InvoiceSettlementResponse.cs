﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response
{
    public class InvoiceSettlementResponse
    {
        public string PaymentRequest { get; set; }
        public long Value { get; set; }
        public bool IsKeysend { get; set; }
        public long Expiry { get; set; }
        public bool Settled { get; set; }
        public long SettledDate { get; set; }
        public long SettledIndex { get; set; }
        public bool Private { get; set; }
        public long AmountInSat { get; set; }
        public int PostId { get; set; }
        public string Email { get; set; }
    }
}
