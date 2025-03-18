using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;
using System;

namespace ClientApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DgiiFileDownloadController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<DgiiFileDownloadController> _logger;

        public DgiiFileDownloadController(ISchedulerFactory schedulerFactory, ILogger<DgiiFileDownloadController> logger)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }

        [HttpPost("trigger-download")]
        [EndpointSummary("Descarga de forma Asincrona el Archivo de la DGII")]
        [EndpointDescription("Descarga de forma Asincrona el Archivo de la DGII. La descarga podria tomar entre 5 a 10 minutos")]
        public async Task<IActionResult> TriggerDownload()
        {
            try
            {
                _logger.LogInformation("Manual trigger of DGII file download requested");

                var scheduler = await _schedulerFactory.GetScheduler();
                await scheduler.TriggerJob(new JobKey("DgiiJob"));

                return Ok(new { message = "DGII file download job triggered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering DGII file download job");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
