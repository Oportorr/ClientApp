using ClientApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static ClientApp.Models.Record;
namespace ClientApp.Services
{

    public class JsonFileService : IJsonFileService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ICacheService _cacheService;
        private readonly ILogger<JsonFileService> _logger;

        public JsonFileService(ICacheService cacheService, ILogger<JsonFileService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }
        public async Task<List<Record>> ReadJsonFileAsync(string filePath)
        {
            return _cacheService.GetOrSet($"file_{filePath}", () =>
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"JSON file not found at path: {filePath}");
                }

                var jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<Record>>(jsonString, _jsonOptions);
            });
        }

        public async Task<Record> GetRecordByIdAsync(string filePath, string id)
        {
            var records = await ReadJsonFileAsync(filePath);
            var record = records.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (record == null)
            {
                throw new KeyNotFoundException($"Record with ID {id} not found");
            }

            return record;
        }

        public async Task<List<Record>> SearchRecordsAsync(string filePath, SearchCriteria criteria)
        {
            var records = await ReadJsonFileAsync(filePath);

            return records.Where(r =>
                (string.IsNullOrEmpty(criteria.Name) || r.Name.Contains(criteria.Name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(criteria.Status) || r.Status.Equals(criteria.Status, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(criteria.Location) || r.Location.Equals(criteria.Location, StringComparison.OrdinalIgnoreCase)) &&
                (!criteria.DateFrom.HasValue || r.DateIn >= criteria.DateFrom) &&
                (!criteria.DateTo.HasValue || r.DateOut <= criteria.DateTo)
            ).ToList();
        }

        public async Task<Record> AddRecordAsync(string filePath, RecordDto recordDto)
        {
            var records = await ReadJsonFileAsync(filePath);

            var newRecord = new Record
            {
                Id = Guid.NewGuid().ToString(),
                Name = recordDto.Name,
                DateIn = recordDto.DateIn,
                DateOut = recordDto.DateOut,
                Status = recordDto.Status,
                Location = recordDto.Location
            };

            records.Add(newRecord);
            await SaveRecordsAsync(filePath, records);

            return newRecord;
        }

        public async Task<Record> UpdateRecordAsync(string filePath, string id, RecordDto recordDto)
        {
            var records = await ReadJsonFileAsync(filePath);
            var existingRecord = records.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (existingRecord == null)
            {
                throw new KeyNotFoundException($"Record with ID {id} not found");
            }

            existingRecord.Name = recordDto.Name;
            existingRecord.DateIn = recordDto.DateIn;
            existingRecord.DateOut = recordDto.DateOut;
            existingRecord.Status = recordDto.Status;
            existingRecord.Location = recordDto.Location;

            await SaveRecordsAsync(filePath, records);

            return existingRecord;
        }
        public async Task<bool> DeleteRecordAsync(string filePath, string id)
        {
            var records = await ReadJsonFileAsync(filePath);
            var recordToRemove = records.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (recordToRemove == null)
            {
                return false;
            }

            records.Remove(recordToRemove);
            await SaveRecordsAsync(filePath, records);

            return true;
        }

        private async Task SaveRecordsAsync(string filePath, List<Record> records)
        {
            var jsonString = JsonSerializer.Serialize(records, _jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString);
            _cacheService.Remove($"file_{filePath}");
        }
    }
}
