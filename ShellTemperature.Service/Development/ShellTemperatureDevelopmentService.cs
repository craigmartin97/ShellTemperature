using ShellTemperature.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShellTemperature.Repository.Interfaces;

namespace ShellTemperature.Service.Development
{
    public class ShellTemperatureDevelopmentService : IShellTemperatureService<ShellTemp>
    {
        private readonly IShellTemperatureRepository<ShellTemp> _shellTemperatureRepository;
        public ShellTemperatureDevelopmentService(IShellTemperatureRepository<ShellTemp> shellTemperatureRepository)
        {
            _shellTemperatureRepository = shellTemperatureRepository;
        }

        public async Task<bool> Create(ShellTemp model)
        {
            bool created = _shellTemperatureRepository.Create(model);
            return await Task.FromResult(created);
        }

        public async Task<IEnumerable<ShellTemp>> GetAll()
        {
            IEnumerable<ShellTemp> shellTemps = _shellTemperatureRepository.GetAll();
            return await Task.FromResult(shellTemps);
        }

        public async Task<ShellTemp> GetItem(Guid id)
        {
            ShellTemp temp = _shellTemperatureRepository.GetItem(id);
            return await Task.FromResult(temp);
        }

        public async Task<bool> Delete(Guid id)
        {
            bool deleted = _shellTemperatureRepository.Delete(id);
            return await Task.FromResult(deleted);
        }

        public async Task<bool> DeleteRange(IEnumerable<ShellTemp> items)
        {
            bool allDeleted = _shellTemperatureRepository.DeleteRange(items);
            return await Task.FromResult(allDeleted);
        }

        public async Task<bool> Update(ShellTemp model)
        {
            bool updated = _shellTemperatureRepository.Update(model);
            return await Task.FromResult(updated);
        }

        public async Task<IEnumerable<ShellTemp>> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null)
        {
            IEnumerable<ShellTemp> temps = _shellTemperatureRepository.GetShellTemperatureData(start, end, deviceName, deviceAddress);
            return await Task.FromResult(temps);
        }
    }
}