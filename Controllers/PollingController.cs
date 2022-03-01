using Microsoft.AspNetCore.Mvc;
using WebAPIPostgresql.Services;

namespace IsBookingOpenedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class PollingController : ControllerBase
    {

        private readonly ILogger<PollingController> _logger;
        private readonly IPollingService _pollingService;

        public PollingController(ILogger<PollingController> logger, IPollingService pollingService)
        {
            _logger = logger;
            _pollingService = pollingService;
        }

        [HttpGet(Name = "StartPolling")]
        public IActionResult StartPolling()
        {
            _logger.LogInformation("Begin Polling");
            return Ok(new { message = _pollingService.StartPollingService() });
        }
    }
}