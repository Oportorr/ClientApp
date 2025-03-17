using ClientApp.Models;
using ClientApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClientApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DGIIController : ControllerBase
    {
        private   string _filePath;
        private readonly IConfiguration _configuration;
        private readonly FileStorage _fileStorageConfig ;


        public DGIIController(IConfiguration configuration,IOptions<FileStorage> filestorageOption)
        {
            // Get the file path from configuration
            _configuration = configuration;
            _fileStorageConfig = filestorageOption.Value;
            _filePath = _fileStorageConfig.DgiiRnc;

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
        [EndpointDescription("Buscar por RNC sin guiones (#########)")]
        public async Task<ActionResult<ContribuyenteDGII>> GetByRNC(string rnc)
        {
            try
            {
                var contribuyentes = await ReadContribuyentesFromFile();
                var contribuyente = contribuyentes.FirstOrDefault(c => c.RNC == rnc);

                if (contribuyente == null)
                {
                    return NotFound($"No se encontró contribuyente con RNC: {rnc}");
                }

                return Ok(contribuyente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading file: {ex.Message}");
            }
        }

        [HttpGet("GetByName/{nombre}")]
        [EndpointDescription("Buscar RNC por Nombre o Razon Social")]
        [EndpointSummary("Buscar RNC por Nombre o Razon Social")]
        [OutputCache]

        public async Task<ActionResult<ContribuyenteDGII>> GetByNombre(string nombre)
        {
            try
            {
                var contribuyentes = await ReadContribuyentesFromFile();
                var contribuyente = contribuyentes.FirstOrDefault(c =>
                c.NombreComercial.Equals(nombre,StringComparison.OrdinalIgnoreCase));


                if (contribuyente == null)
                {
                    return NotFound($"No se encontró contribuyente con RNC: {nombre}");
                }

                return Ok(contribuyente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading file: {ex.Message}");
            }
        }

        [HttpGet("search/{term}")]
        [EndpointDescription("Valores que contienen RNC o nombre, primeras 10 ocurrencias")]
        [EndpointSummary("Valores que contienen RNC o nombre, primeras 10 ocurrencias")]
        [OutputCache]
        public async Task<ActionResult<IEnumerable<ContribuyenteDGII>>> Search(string term)
        {
            try
            {
                _filePath = _fileStorageConfig.DgiiRnc;

                var contribuyentes = await ReadContribuyentesFromFile();
                var results = contribuyentes.Where(c =>
                    c.NombreComercial.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.RNC.Contains(term))
                    .Take(10);

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading file: {ex.Message}");
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