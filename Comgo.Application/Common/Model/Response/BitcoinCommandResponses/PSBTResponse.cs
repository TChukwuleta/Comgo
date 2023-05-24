namespace Comgo.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class PSBTResponse
    {
        public string psbt { get; set; }
        public double fee { get; set; }
        public int changepos { get; set; }
    }
}
