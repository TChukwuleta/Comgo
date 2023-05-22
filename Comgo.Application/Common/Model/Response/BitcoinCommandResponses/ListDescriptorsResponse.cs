using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class ListDescriptorsResponse
    {
        public string wallet_name { get; set; }
        public List<DescriptorsResponse> descriptors { get; set; }
    }

    public class DescriptorsResponse
    {
        public string desc { get; set; }
        public int timestamp { get; set; }
        public bool active { get; set; }
        public bool @internal { get; set; }
        public List<int> range { get; set; }
        public int next { get; set; }
    }
}
