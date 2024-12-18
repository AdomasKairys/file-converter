using file_converter_api.Controllers;
using nClam;
using System.Net;
using System.Security.Claims;

namespace file_converter_api.Services
{
    public class ClamAVService
    {
        private readonly ClamClient _clamClient;
        private readonly ILogger<ClamClient> _logger;

        public ClamAVService(ILogger<ClamClient> logger)
        {
            _logger = logger;
            while (true)
            {
                _logger.LogInformation("Connecting to ClamAV service.");

                _clamClient = new ClamClient("host.docker.internal", 3310);
                var pingResult = _clamClient.TryPingAsync();
                pingResult.Wait();
                if (!pingResult.Result)
                {
                    _logger.LogError("Could not connect to ClamAV service retrying.", _clamClient);
                    Thread.Sleep(2000);
                    continue;
                }
                break;
            }
        }
        public async Task<bool> ScanFileForViruses(IFormFile formFile)
        {
            using (var file = formFile.OpenReadStream())
            {
                file.Position = 0; // Ensure the stream is at the beginning
                var scanResult = await _clamClient.SendAndScanFileAsync(file);
                _logger.LogInformation($"scan result {scanResult.Result}", scanResult);
                return scanResult.Result == ClamScanResults.Clean;
            }
        }

    }
}
