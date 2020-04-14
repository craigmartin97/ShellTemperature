using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ShellTemperature.Service
{
    public abstract class BaseService
    {
        protected readonly HttpClient _httpClient;

        protected BaseService(HttpClient client)
        {
            _httpClient = client;
        }

        protected StringContent GetStringContent(object obj)
            => new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
    }
}