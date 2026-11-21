using System.Diagnostics;

namespace DemoWeb.Tests;

/// <summary>
/// Menjalankan aplikasi DemoWeb sekali sebelum semua test, lalu mematikannya setelah selesai.
/// Jika aplikasi sudah berjalan (misal dijalankan manual), server tidak distart ulang.
/// Set environment variable BASE_URL untuk menguji server lain.
/// </summary>
[SetUpFixture]
public class AppServer
{
    public static string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5123";

    private static Process? _process;

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        if (await IsUpAsync()) return;

        var projectDir = Path.Combine(FindRepoRoot(), "src", "DemoWeb");
        _process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectDir}\" --no-launch-profile --urls {BaseUrl}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        })!;
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            if (_process.HasExited)
                throw new InvalidOperationException("DemoWeb berhenti sebelum siap. Coba jalankan: dotnet run --project src/DemoWeb");
            if (await IsUpAsync()) return;
            await Task.Delay(500);
        }
        throw new TimeoutException($"DemoWeb tidak merespons di {BaseUrl}");
    }

    [OneTimeTearDown]
    public void Stop()
    {
        if (_process is { HasExited: false })
            _process.Kill(entireProcessTree: true);
        _process?.Dispose();
    }

    private static async Task<bool> IsUpAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            return (await http.GetAsync(BaseUrl)).IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "PlaywrightDemo.slnx")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("PlaywrightDemo.slnx tidak ditemukan");
    }
}
