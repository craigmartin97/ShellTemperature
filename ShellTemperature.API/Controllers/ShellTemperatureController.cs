using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;

namespace ShellTemperature.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShellTemperatureController : ControllerBase
    {
        private readonly IShellTemperatureRepository<ShellTemp> _shellTemperatureRepository;

        public ShellTemperatureController(IShellTemperatureRepository<ShellTemp> shellTemperatureRepository)
        {
            _shellTemperatureRepository = shellTemperatureRepository;
        }

        [HttpPost]
        public IActionResult Create(ShellTemp shellTemp)
        {
            if(shellTemp == null)
                return BadRequest("Null model");

            _shellTemperatureRepository.Create(shellTemp);
            return Ok();
        }
    }
}