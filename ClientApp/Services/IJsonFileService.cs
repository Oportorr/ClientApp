using ClientApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ClientApp.Models.Record;

namespace ClientApp.Services
{
    public interface IJsonFileService
    {
        Task<List<Record>> ReadJsonFileAsync(string filePath);
        Task<Record> GetRecordByIdAsync(string filePath, string id);
        Task<List<Record>> SearchRecordsAsync(string filePath, SearchCriteria criteria);
        Task<Record> AddRecordAsync(string filePath, RecordDto recordDto);
        Task<Record> UpdateRecordAsync(string filePath, string id, RecordDto recordDto);
        Task<bool> DeleteRecordAsync(string filePath, string id);
    }
}
