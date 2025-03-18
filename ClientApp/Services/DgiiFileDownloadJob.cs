
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp.Services
{
   

    public class DgiiFileDownloadJob : IJob
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DgiiFileDownloadJob> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DgiiFileDownloadJob(
            IConfiguration configuration,
            ILogger<DgiiFileDownloadJob> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                // Get settings from configuration
                string downloadUrl = _configuration["DgiiSettings:FileUrl"] ?? "https://dgii.gov.do/app/WebApps/Consultas/RNC/DGII_RNC.zip";
                string downloadFolder = _configuration["DgiiSettings:DownloadFolder"] ?? "Downloads";

                _logger.LogInformation("Starting DGII file download job at {time}", DateTimeOffset.Now);

                // Ensure the download directory exists
                Directory.CreateDirectory(downloadFolder);

                // Delete all existing files in the download folder
                foreach (var file in Directory.GetFiles(downloadFolder))
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted file: {file}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting file: {file}", file);
                    }
                }

                // Download the zip file
                string zipPath = Path.Combine(downloadFolder, "DGII_RNC.zip");
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(8); // Set a reasonable timeout

                    _logger.LogInformation("Downloading file from {url}", downloadUrl);
                    var response = await httpClient.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                    _logger.LogInformation("File downloaded successfully to {path}", zipPath);
                }

                // Extract the zip file
                _logger.LogInformation("Extracting zip file");
                ZipFile.ExtractToDirectory(zipPath, downloadFolder, true);
                _logger.LogInformation("Extraction completed successfully");

                // Optionally, delete the zip file after extraction
                File.Delete(zipPath);
                _logger.LogInformation("Deleted zip file after extraction");

                _logger.LogInformation("DGII file download job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during DGII file download job");
                throw;
            }
        }
    }
}
