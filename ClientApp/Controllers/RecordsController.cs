using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ClientApp.Models;
using ClientApp.Services;
using System.Text.Json;

namespace ClientApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordsController : ControllerBase
    {

        private readonly IJsonFileService _jsonFileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecordsController> _logger;

        public RecordsController(
            IJsonFileService jsonFileService,
            IConfiguration configuration,
            ILogger<RecordsController> logger)
        {
            _jsonFileService = jsonFileService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Record>>> GetRecords(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return BadRequest("Filename cannot be empty");
                }

                var filePath = Path.Combine(_configuration["FileStorage:Path"], fileName);
                _logger.LogInformation("Reading JSON file: {FilePath}", filePath);

                var records = await _jsonFileService.ReadJsonFileAsync(filePath);
                return Ok(records);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "File not found: {FileName}", fileName);
                return NotFound($"File not found: {fileName}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON format in file: {FileName}", fileName);
                return BadRequest("Invalid JSON format in file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", fileName);
                return StatusCode(500, "Internal server error while processing the file");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Record>> GetRecordById(string fileName, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return BadRequest("Filename cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("ID cannot be empty");
                }

                var filePath = Path.Combine(_configuration["FileStorage:Path"], fileName);
                _logger.LogInformation("Searching for record with ID {Id} in file: {FilePath}", id, filePath);

                var record = await _jsonFileService.GetRecordByIdAsync(filePath, id);
                return Ok(record);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "File not found: {FileName}", fileName);
                return NotFound($"File not found: {fileName}");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Record not found: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON format in file: {FileName}", fileName);
                return BadRequest("Invalid JSON format in file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", fileName);
                return StatusCode(500, "Internal server error while processing the file");
            }
        }



    }
}
