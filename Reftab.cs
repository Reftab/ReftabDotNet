using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Reftab
{
    /// <summary>
    /// Used to connect to Reftab API and conduct HTTP requests
    /// </summary>
    public class Api
    {
        private string publicKey;
        private string secretKey;
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Initialize an API connector to make requests to Reftab.
        /// </summary>
        /// <param name="publicKeyValue">Public Key generated in Reftab to connect to API.</param>
        /// <param name="secretKeyValue">Secret Key generated in Reftab to connect to API.</param>
        public Api(string publicKeyValue, string secretKeyValue)
        {
            publicKey = publicKeyValue;
            secretKey = secretKeyValue;
        }

        /// <summary>
        /// The main method for communicating with the Reftab API.
        /// </summary>
        /// <param name="method">GET, POST, PUT, DELETE HTTP method to communicate with Reftab API</param>
        /// <param name="endpoint">The endpoint to communicate with, e.g. assets, assets/USNY01, loans</param>
        /// <param name="body">For GET requests set to empty string, for POST/PUT requests send a serialized JSON string.</param>
        /// <returns></returns>
        public async Task<string> Request(string method, string endpoint, string body)
        {
            string url = "https://www.reftab.com/api/" + endpoint;
            string json = string.Empty;

            HttpRequestMessage message = new HttpRequestMessage();

            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string contentMD5 = "";
            string contentType = "";
            if (body != "")
            {
                contentType = "application/json";
                MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(body);
                byte[] md5hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < md5hashBytes.Length; i++)
                {
                    sb.Append(md5hashBytes[i].ToString("X2"));
                }
                contentMD5 = sb.ToString().ToLower();
            }
            string signatureToSign = method.ToUpper() + '\n' + contentMD5 + '\n' + contentType + '\n' + now + '\n' + url;

            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] textBytes = encoding.GetBytes(signatureToSign);
            Byte[] keyBytes = encoding.GetBytes(secretKey);
            Byte[] hashBytes;
            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            string signed = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(signed);
            signed = System.Convert.ToBase64String(plainTextBytes);

            message.RequestUri = new Uri(url);
            message.Method = new HttpMethod(method);
            message.Headers.Add("x-rt-date", now);
            message.Headers.Add("Authorization", "RT " + publicKey + ":" + signed);
            if (body != "")
            {
                message.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await client.SendAsync(message);

            json = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
            {
                throw new ReftabException(json);
            }
            return json;

        }
    }

    [Serializable]
    class ReftabException : Exception
    {
        public ReftabException() { }
        public ReftabException(string message) : base(String.Format("Reftab Error: {0}", message)) { }
    }
}