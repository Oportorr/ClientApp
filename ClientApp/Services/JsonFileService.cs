using System;
using System.Text.Json;
using ClientApp.Models;
namespace ClientApp.Services
{
    public class JsonFileService : IJsonFileService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFileService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<List<Record>> ReadJsonFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found at path: {filePath}");
            }

            if (!Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("File must be a JSON file");
            }

            var jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<Record>>(jsonString, _jsonOptions);
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
    }
}
