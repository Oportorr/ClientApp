using ClientApp.Models;

namespace ClientApp.Services
{
    public interface IJsonFileService
    {
        Task<List<Record>> ReadJsonFileAsync(string filePath);
        Task<Record> GetRecordByIdAsync(string filePath, string id);
    }
}
