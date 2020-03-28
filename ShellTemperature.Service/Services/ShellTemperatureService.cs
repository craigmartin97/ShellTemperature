using ShellTemperature.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShellTemperature.Service.Services
{
    public class ShellTemperatureService : BaseService, IShellTemperatureService<ShellTemp>
    {
        private readonly string baseAddress = "api/ShellTemperature";

        public ShellTemperatureService(HttpClient client) : base(client) { }

        public async Task<bool> Create(ShellTemp model)
        {
            using HttpResponseMessage responseMessage = await _httpClient.PostAsync(baseAddress, GetStringContent(model));
            try
            {
                return responseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<ShellTemp>> GetAll()
        {
            using HttpResponseMessage responseMessage = await _httpClient.GetAsync(baseAddress);
            return responseMessage.IsSuccessStatusCode
                ? await responseMessage.Content.ReadAsAsync<IEnumerable<ShellTemp>>()
                : null;
        }

        public async Task<ShellTemp> GetItem(Guid id)
        {
            using HttpResponseMessage responseMessage = await _httpClient.GetAsync(baseAddress + id);
            return responseMessage.IsSuccessStatusCode ? await responseMessage.Content.ReadAsAsync<ShellTemp>() : null;
        }

        public async Task<IEnumerable<ShellTemp>> GetShellTemperatureData(DateTime start, DateTime end)
        {
            using HttpResponseMessage responseMessage = await _httpClient.GetAsync(baseAddress);
            return responseMessage.IsSuccessStatusCode
                ? await responseMessage.Content.ReadAsAsync<IEnumerable<ShellTemp>>()
                : null;
        }

        public async Task<IEnumerable<ShellTemp>> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null)
        {
            using HttpResponseMessage responseMessage = await _httpClient.GetAsync(baseAddress);
            return responseMessage.IsSuccessStatusCode
                ? await responseMessage.Content.ReadAsAsync<IEnumerable<ShellTemp>>()
                : null;
        }

        public async Task<bool> Delete(Guid id)
        {
            using HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(baseAddress + id);
            try
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    string ex = await responseMessage.Content.ReadAsStringAsync();
                    throw new Exception(ex);
                }
                return responseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<bool> DeleteRange(IEnumerable<ShellTemp> items)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(ShellTemp model)
        {
            using HttpResponseMessage responseMessage = await _httpClient.PutAsync(baseAddress + model.Id, GetStringContent(model));
            try
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    string ex = await responseMessage.Content.ReadAsStringAsync();
                    throw new Exception(ex);
                }
                return responseMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}