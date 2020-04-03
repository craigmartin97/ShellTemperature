using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ShellTemperature.Data;

namespace ShellTemperature.Service.Live
{
    public class ShellTemperatureLiveService : BaseService, IShellTemperatureService<ShellTemp>
    {
        private readonly string baseAddress = "api/ShellTemperature/";

        public ShellTemperatureLiveService(HttpClient client) : base(client) { }

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

        public async Task<IEnumerable<ShellTemp>> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null)
        {
            string startString = start.ToString("yyyy-MM-dd HH:mm:ss");

            string endString = end.ToString("yyyy-MM-dd HH:mm:ss");

            string queryFilter = baseAddress + "GetBetweenDates?start=" + startString + "&end=" + endString;
            if (!string.IsNullOrEmpty(deviceName))
                queryFilter += "&deviceName=" + deviceName;
            if (!string.IsNullOrEmpty(deviceAddress))
                queryFilter += "&deviceAddress=" + deviceAddress;

            using (HttpResponseMessage responseMessage = await _httpClient.GetAsync(queryFilter))
            {
                return responseMessage.IsSuccessStatusCode
                    ? await responseMessage.Content.ReadAsAsync<IEnumerable<ShellTemp>>()
                    : null;
            }
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

        public async Task<bool> DeleteRange(IEnumerable<ShellTemp> items)
        {
            using HttpResponseMessage responseMessage = await _httpClient.PostAsync(baseAddress + "DeleteRange", GetStringContent(items));
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