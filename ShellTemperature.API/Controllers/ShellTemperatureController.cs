using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;

namespace ShellTemperature.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShellTemperatureController : ControllerBase
    {
        #region Fields
        /// <summary>
        /// Shell temperature repository to engage with the shell temperature table of the database
        /// </summary>
        private readonly IRepository<ShellTemp> _shellTempRepository;
        #endregion

        #region Constructors

        public ShellTemperatureController(IRepository<ShellTemp> shellTempRepository)
        {
            _shellTempRepository = shellTempRepository;
        }
        #endregion

        #region Create
        /// <summary>
        /// Create a new shell temperature record in the database
        /// </summary>
        /// <param name="record">The new record to create</param>
        /// <returns>Returns a OK if the request was a success</returns>
        [HttpPost]
        public async Task<IActionResult> Create(ShellTemperatureRecordAPI record)
        {
            try
            {
                if (!record.RecordedDateTime.HasValue)
                    record.RecordedDateTime = DateTime.Now;

                ShellTemp shellTemp = new ShellTemp(record.Id, record.Temperature, (DateTime)record.RecordedDateTime, record.Latitude, record.Longitude, record.DeviceInfo);
                bool result = await _shellTempRepository.Create(shellTemp);

                if (!result)
                    return BadRequest();

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest();
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Get all of the shell temperature readings from the database
        /// </summary>
        /// <returns>Returns a collection of shell temperature readings from the database</returns>
        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    ShellTemp[] allDataReadings = await _shellTempRepository.GetAll();

        //    if (allDataReadings != null && allDataReadings.Length > 0)
        //        return Ok(allDataReadings);

        //    return BadRequest("No data could be found");
        //}
        #endregion
    }
}