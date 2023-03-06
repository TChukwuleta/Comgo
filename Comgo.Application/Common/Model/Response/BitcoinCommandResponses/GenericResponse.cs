using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class GenericResponse
    {
        public string result { get; set; }
        public string error { get; set; }
        public string id { get; set; }
    }
}
