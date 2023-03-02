using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Infrastructure.Services
{
    public class BitcoinCoreClient : IBitcoinCoreClient
    {
        private readonly IConfiguration _config;
        public ApiRequestDto apiRequestDto { get; set; }
        private readonly string serverIp;
        private readonly string username;
        private readonly string password;
        public BitcoinCoreClient(IConfiguration config)
        {
            _config = config;
            apiRequestDto = new ApiRequestDto();
            serverIp = _config["Bitcoin:URl"];
            username = _config["Bitcoin:username"];
            password = _config["Bitcoin:password"];
        }

        public async Task<string> BitcoinRequestServer(string methodName, List<JToken> parameters)
        {
            string response = default;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverIp);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";

                JObject joe = new JObject();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltest"));
                joe.Add(new JProperty("method", methodName));
                JArray props = new JArray();
                foreach (var parameter in parameters)
                {
                    props.Add(parameter);
                }
                joe.Add(new JProperty("params", props));

                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Close();

                StreamReader streamReader = null; 
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName, List<string> parameters)
        {
            try
            {
                return await BitcoinRequestServer(methodName, parameters.Select(c => new JValue(c)).ToList<JToken>());
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName, string parameters)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverIp);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";
                string responseValue = string.Empty;
                JObject joe = new JObject();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltext"));
                joe.Add(new JProperty("method", methodName));
                JArray props = new JArray();
                props.Add(parameters);
                joe.Add(new JProperty("params", props));
                // Serialize json for request
                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream dataStream = webRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                // Deserialize the response
                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                responseValue = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(responseValue).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName)
        {
            string response = default;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverIp);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";
                JObject joe = new JObject();
                JArray props = new JArray();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltext"));
                joe.Add(new JProperty("method", methodName));
                joe.Add(new JProperty("params", props));
                string s = JsonConvert.SerializeObject(joe);
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = bytes.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
