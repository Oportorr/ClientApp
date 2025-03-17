using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ClientApp.Models;
using ClientApp.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using static ClientApp.Models.Record;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;

namespace ClientApp.Controllers
{
    //  [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Record>>> GetRecords(string fileName)
        {
            try
            {
                var filePath = GetValidatedFilePath(fileName);
                var records = await _jsonFileService.ReadJsonFileAsync(filePath);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return HandleException(ex, fileName);
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Record>> GetRecordById(string fileName, string id)
        {
            try
            {
                var filePath = GetValidatedFilePath(fileName);
                var record = await _jsonFileService.GetRecordByIdAsync(filePath, id);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return HandleException(ex, fileName);
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Record>>> SearchRecords(string fileName, [FromBody] SearchCriteria criteria)
        {
            try
            {
                var filePath = GetValidatedFilePath(fileName);
                var records = await _jsonFileService.SearchRecordsAsync(filePath, criteria);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return HandleException(ex, fileName);
            }
        }

        [HttpPost]
        [EndpointDescription("Get a subscription by its ID")]
        [EndpointSummary("Get a subscription by its ID")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Record>> AddRecord(string fileName, [FromBody] RecordDto recordDto)
        {
            try
            {
                var filePath = GetValidatedFilePath(fileName);
                var newRecord = await _jsonFileService.AddRecordAsync(filePath, recordDto);
                return CreatedAtAction(nameof(GetRecordById), new { id = newRecord.Id, fileName }, newRecord);
            }
            catch (Exception ex)
            {
                return HandleException(ex, fileName);
            }
        }

        [HttpPut("{id}")]
      //  [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Record>> UpdateRecord(string fileName, string id, [FromBody] RecordDto recordDto)
        {
            try
            {
                var filePath = GetValidatedFilePath(fileName);
                var updatedRecord = await _jsonFileService.UpdateRecordAsync(filePath, id, recordDto);
                return Ok(updatedRecord);
            }
            catch (Exception ex)
            {
                return HandleException(ex, fileName);
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteRecord(string fileName, string id)
        {
            try
            {
                var filePath = GetValidatedFilePath(fileName);
                var result = await _jsonFileService.DeleteRecordAsync(filePath, id);
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, fileName);
            }
        }
        private string GetValidatedFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Filename cannot be empty");
            }

            return Path.Combine(_configuration["FileStorage:Path"], fileName);
        }

        private ActionResult HandleException(Exception ex, string fileName)
        {
            _logger.LogError(ex, "Error processing file: {FileName}", fileName);

            return ex switch
            {
                FileNotFoundException => NotFound($"File not found: {fileName}"),
                KeyNotFoundException => NotFound(ex.Message),
                ArgumentException => BadRequest(ex.Message),
                JsonException => BadRequest("Invalid JSON format in file"),
                _ => StatusCode(500, "Internal server error while processing the file")
            };
        }
    }
}
