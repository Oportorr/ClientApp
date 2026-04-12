using ClientApp.Models;
using ClientApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        [EndpointSummary("Buscar por RNC o cedula")]
        [EndpointDescription("Buscar por RNC o cedula sin guiones formato (#########)")]
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

                    var nombreNormalizado = NormalizarTexto(nombre.Trim());

                    // DEBUG - quitar después
                    _logger.LogInformation("Nombre original: '{Original}' | Normalizado: '{Normalizado}'",
                        nombre, nombreNormalizado);




                    var contribuyentes = await ReadContribuyentesFromFile();




                    //var contribuyente = contribuyentes.FirstOrDefault(c =>
                    //c.NombreCompleto.Contains(nombre.Trim(), StringComparison.OrdinalIgnoreCase));
                    var contribuyente = contribuyentes.FirstOrDefault(c => c.NombreCompletoNormalizado
                          .Contains(nombreNormalizado, StringComparison.OrdinalIgnoreCase));


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
        [EndpointDescription("Buscar Contribuyentes por Nombre Comercial (máximo 10 resultados)")]
        [EndpointSummary("Buscar Contribuyentes por Nombre Comercial (máximo 10 resultados")]
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
                    //_filePath = _fileStorageConfig.DgiiRnc;
                    var termNormalizado = NormalizarTexto(term.Trim());

                    var contribuyentes = await ReadContribuyentesFromFile();
                    var results = contribuyentes.Where(c =>
                        c.NombreCompletoNormalizado.Contains(termNormalizado.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    c.RNC.Contains(termNormalizado))
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




        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            // Descompone caracteres especiales (ej: Ñ → N + combinación)
            var normalizado = texto.Normalize(NormalizationForm.FormD);

            // Elimina los caracteres diacríticos EXCEPTO la Ñ
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }




        private async Task<List<ContribuyenteDGII>> ReadContribuyentesFromFile()
        {
            var contribuyentes = new List<ContribuyenteDGII>();

            // using (var reader = new StreamReader(_filePath))
            //using (var reader = new StreamReader(_filePath, Encoding.UTF8))
            using (var reader = new StreamReader(_filePath, Encoding.Latin1))

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
                            RNC = fields[0].Trim(),
                            NombreCompleto = fields[1].Trim(),
                            NombreComercial = fields[2].Trim(),
                            Actividad = fields[3].Trim(),
                            FechaRegistro = fields[8].Trim(),
                            Estado = fields[9].Trim(),
                            Categoria = fields[10].Trim(),

                            NombreCompletoNormalizado = NormalizarTexto(fields[1].Trim()),
                            NombreComercialNormalizado = NormalizarTexto(fields[2].Trim())
                        });
                    }
                }
            }

            return contribuyentes;
        }
    }
}