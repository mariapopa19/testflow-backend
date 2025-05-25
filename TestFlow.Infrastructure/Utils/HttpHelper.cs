using System.Text;
using Newtonsoft.Json.Linq;

namespace TestFlow.Infrastructure.Utils
{
    public static class HttpHelper
    {
        public static async Task<HttpResponseMessage> SendRequestAsync(IHttpClientFactory factory, string url, string method, JObject body)
        {
            var client = factory.CreateClient();
            var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");

            return method.ToUpper() switch
            {
                "POST" => await client.PostAsync(url, content),
                "PUT" => await client.PutAsync(url, content),
                "DELETE" => await client.DeleteAsync(url),
                "GET" => await client.GetAsync(url),
                _ => throw new HttpRequestException($"Unsupported HTTP method: {method}")
            };
        }
    }
}
