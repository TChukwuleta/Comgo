﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailMessage(string body);
    }
}
