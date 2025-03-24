using ClientApp.Models;
using ClientApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClientApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DGIIController : ControllerBase
    {
        private   string _filePath;
        private readonly IConfiguration _configuration;
        private readonly FileStorage _fileStorageConfig;
        private readonly ILogger<DGIIController> _logger;


        public DGIIController(IConfiguration configuration,IOptions<FileStorage> filestorageOption, ILogger<DGIIController> ilogger)
        {
                                                                                                                                                           
            // Get the file path from configuration
            _configuration = configuration;
            _fileStorageConfig = filestorageOption.Value;
            _filePath = _fileStorageConfig.DgiiRnc;
            _logger = ilogger;

        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContribuyenteDGII>>> GetAll()
        {
            try

            {
                var contribuyentes = await ReadContribuyentesFromFile();
                return Ok(contribuyentes);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error reading file: {ex.Message}");
            }
        }

        [HttpGet("{rnc}")]
        [EndpointSummary("Buscar por RNC")]
        [EndpointDescription("Buscar por RNC sin guiones formato (#########)")]
        public async Task<ActionResult<ContribuyenteDGII>> GetByRNC(string rnc)
        {
            // Get the client IP address
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // If behind a reverse proxy, check X-Forwarded-For
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? clientIp;
            }

            try
            {
                // Add the client IP to log context (ensuring it's within the logging scope)
                using (LogContext.PushProperty("ClientIP", clientIp))
                {
                 

                    var contribuyentes = await ReadContribuyentesFromFile();
                    var contribuyente = contribuyentes.FirstOrDefault(c => c.RNC == rnc);

                    if (contribuyente == null)
                    {
                        _logger.LogWarning("No se encontró contribuyente con RNC: {RNC} from IP: {ClientIP}", rnc, clientIp);

                        return NotFound($"No se encontró contribuyente con RNC: {rnc}");
                    }

                    //_logger.LogInformation($"Search request received with RNC : {rnc} from IP: {clientIp}");
                    _logger.LogInformation("Successful search for RNC: {RNC} from IP: {ClientIP}", rnc, clientIp);

                    return Ok(contribuyente);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing search request for RNC: {RNC} from IP: {ClientIP}",rnc,clientIp);
                return StatusCode(500, $"Error reading file: {ex.Message}");
            }
        }




        [HttpGet("GetByName/{nombre}")]
        [EndpointDescription("Buscar RNC por Nombre o Razon Social")]
        [EndpointSummary("Buscar RNC por Nombre o Razon Social")]
        [OutputCache]

        public async Task<ActionResult<ContribuyenteDGII>> GetByNombre(string nombre)
        {

            // Get the client IP address
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // If you're behind a proxy, use the X-Forwarded-For header
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? clientIp;
            }

            // Use LogContext to add the IP to the log context
            using (LogContext.PushProperty("ClientIP", clientIp))
            {
                try
                {

                   

                    var contribuyentes = await ReadContribuyentesFromFile();
                    var contribuyente = contribuyentes.FirstOrDefault(c =>
                    c.NombreComercial.Equals(nombre, StringComparison.OrdinalIgnoreCase));


                    if (contribuyente == null)
                    {
                        _logger.LogWarning("No se encontró contribuyente con : {NOMBRE} from IP: {ClientIP}", nombre, clientIp);

                        return NotFound($"No se encontró contribuyente con RNC: {nombre}");
                    }

                    _logger.LogInformation("Search request received with Nombre: {NOMBRE} from IP: {ClientIP}", nombre,clientIp);

                    return Ok(contribuyente);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing search request");
                    return StatusCode(500, $"Error reading file: {ex.Message}");
                }
            }
        }

        [HttpGet("search/{term}")]
        [EndpointDescription("Valores que contienen RNC o nombre, primeras 10 ocurrencias")]
        [EndpointSummary("Valores que contienen RNC o nombre, primeras 10 ocurrencias")]
        [OutputCache]
        public async Task<ActionResult<IEnumerable<ContribuyenteDGII>>> Search(string term)
        {

            // Get the client IP address
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // If you're behind a proxy, use the X-Forwarded-For header
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? clientIp;
            }

            // Use LogContext to add the IP to the log context
            using (LogContext.PushProperty("ClientIP", clientIp))
            {

                try
                {
                    _filePath = _fileStorageConfig.DgiiRnc;

                    var contribuyentes = await ReadContribuyentesFromFile();
                    var results = contribuyentes.Where(c =>
                        c.NombreComercial.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.RNC.Contains(term))
                    .Take(10);

                    _logger.LogInformation("Search request received with Term: {Term} from IP: {ClientIP}", term,clientIp);

                    return Ok(results);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing search request not found {Term} from IP:ClientIP{}",term,clientIp);
                    return StatusCode(500, $"Error reading file: {ex.Message}");
                }
            }
        }


 
        private async Task<List<ContribuyenteDGII>> ReadContribuyentesFromFile()
        {
            var contribuyentes = new List<ContribuyenteDGII>();

            using (var reader = new StreamReader(_filePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var fields = line.Split('|');
                    if (fields.Length >= 11)
                    {
                        contribuyentes.Add(new ContribuyenteDGII
                        {
                            RNC = fields[0],
                            NombreCompleto = fields[1],
                            NombreComercial = fields[2],
                            Actividad = fields[3],
                            //Campo5 = fields[4],
                            //Campo6 = fields[5],
                            //Campo7 = fields[6],
                            //Campo8 = fields[7],
                            FechaRegistro = fields[8],
                            Estado = fields[9],
                            Categoria = fields[10]
                        });
                    }
                }
            }

            return contribuyentes;
        }
    }
}