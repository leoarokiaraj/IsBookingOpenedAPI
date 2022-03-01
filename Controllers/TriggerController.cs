using IsBookingOpenedAPI.Entities;
using IsBookingOpenedAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IsBookingOpenedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class TriggerController : ControllerBase
    {

        private readonly ILogger<TriggerController> _logger;
        private readonly ITriggerService _triggerService;

        public TriggerController(ILogger<TriggerController> logger, ITriggerService triggerService)
        {
            _logger = logger;
            _triggerService = triggerService;
        }

        [HttpPost("AddUpdateTrigger")]
        public ContentResult AddUpdateTrigger([FromBody] Trigger triggerObj)
        {
            ContentResult response = new ContentResult()
            {
                ContentType = "text/plain"
            };
            try
            {
                _logger.LogInformation("Beging AddUpdateTrigger Controller");
                int ret = _triggerService.AddUpdateTrigger(triggerObj);
                _logger.LogInformation("Beging AddUpdateTrigger Controller");

                switch(ret)
                {
                    case (int)TriggerResponseStatus.Failure:
                        response.StatusCode = 500;
                        response.Content = $"Something went wrong {ret}";
                        break;
                    case (int)TriggerResponseStatus.RecordNotExist:
                        response.StatusCode = 404;
                        response.Content = $"Record does not exist {ret}";
                        break;
                    default:
                        response.StatusCode = 200;
                        response.Content = $"Success {ret}";
                        break;
                }
                
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Content = ex.Message;
                return response;
            }
            
        }

        [HttpGet("GetAllTriggers")]
        public IEnumerable<Trigger> GetAllTriggers()
        {
            try
            {
                _logger.LogInformation("Beging GetAllTriggers Controller");
                return _triggerService.GetAllTriggers();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error GetAllTriggers Controller {ex}");
                return null;
            }

        }

        [HttpDelete("DeleteTrigger")]
        public ContentResult DeleteTrigger([FromQuery(Name = "triggerId")] int triggerId)
        {
            ContentResult response = new ContentResult()
            {
                ContentType = "text/plain"
            };
            try
            {
                _logger.LogInformation("Beging DeleteTrigger Controller");
                int ret = _triggerService.DeleteTrigger(triggerId);

                switch (ret)
                {
                    case (int)TriggerResponseStatus.Failure:
                        response.StatusCode = 500;
                        response.Content = $"Something went wrong {ret}";
                        break;
                    case (int)TriggerResponseStatus.RecordNotExist:
                        response.StatusCode = 404;
                        response.Content = $"Record does not exist {ret}";
                        break;
                    default:
                        response.StatusCode = 200;
                        response.Content = $"Success {ret}";
                        break;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Content = ex.Message;
                return response;
            }

        }
    }
}