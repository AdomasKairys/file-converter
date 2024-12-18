namespace file_converter_client.Services
{
public class TempFileCleanupService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly string _tempDirectory = Path.GetTempPath();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CleanupFiles, null, TimeSpan.Zero, TimeSpan.FromMinutes(5)); // Adjust cleanup frequency as needed
        return Task.CompletedTask;
    }

    private void CleanupFiles(object state)
    {
        var now = DateTime.Now;
        foreach (var file in Directory.GetFiles(_tempDirectory))
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.LastAccessTime < now.AddHours(-1)) // Delete files older than 1 hour
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted file: {fileInfo.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file {fileInfo.Name}: {ex.Message}");
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
}
