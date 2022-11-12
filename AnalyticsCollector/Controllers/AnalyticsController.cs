using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Analytics.Database;
using AnalyticsCollector.DTOs;
using AnalyticsCollector.Mappers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;

namespace AnalyticsCollector.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsEntryDtoMapper _mapper;
        private readonly IAnalyticsRepository _repository;

        public AnalyticsController(
            IAnalyticsEntryDtoMapper mapper,
            IAnalyticsRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        
        [HttpPost("startSession")]
        [EnableCors("MyPolicy")]
        public async Task<ActionResult> StartSession([FromBody] CreateSessionDto dto)
        {
            var sessionId = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();

            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            var entry = _mapper.ToAnalyticsEntry(dto, sessionId, userAgent, ipAddress);
            await _repository.SaveSession(entry);

            return Ok(sessionId);
        }

        /// <summary>
        /// Endpoint to push analytics events. 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("pushEvents")]
        [EnableCors("MyPolicy")]
        public async Task<ActionResult> PushEvents([FromBody] AddEventsDto dto)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            var entry = _mapper.ToAnalyticsEntry(dto, userAgent, ipAddress);
            await _repository.SaveEvents(dto.SessionId, entry.Events);

            return NoContent();
        }

        /// <summary>
        /// It must accept post requests and content-type text to support
        /// navigator.sendBeacon() calls.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("pushEvents")]
        [EnableCors("MyPolicy")]
        public async Task<ActionResult> PushEventsForBeacon()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var dto = await reader.ReadToEndAsync();
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.ConfigureForNodaTime(NodaTime.DateTimeZoneProviders.Tzdb);
                    var analyticsEntry = JsonConvert.DeserializeObject<AddEventsDto>(dto, settings);

                    return await PushEvents(analyticsEntry);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
    }
}
