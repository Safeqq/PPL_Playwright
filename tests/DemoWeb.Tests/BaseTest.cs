using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;

namespace DemoWeb.Tests;

// Menyalakan web app DemoWeb SEKALI sebelum semua test, lalu mematikannya di akhir.
[SetUpFixture]
public class AppServer
{
    public static string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5123";

    private static Process? _process;

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        if (await IsUpAsync()) return; // sudah menyala (dijalankan manual)

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

// Induk semua test. PageTest otomatis memberi Browser, Context, dan Page baru di SETIAP test.
// Jangan menaruh [Test] di sini, karena akan ikut diwarisi semua class test.
public abstract class BaseTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = AppServer.BaseUrl, // cukup tulis GotoAsync("/Login")
        Locale = "id-ID",
        ViewportSize = new() { Width = 1280, Height = 720 },
    };

    // Tracing: rekam setiap test, simpan .zip hanya kalau gagal
    [SetUp]
    public async Task MulaiTracingAsync()
    {
        await Context.Tracing.StartAsync(new()
        {
            Title = TestContext.CurrentContext.Test.Name,
            Screenshots = true,
            Snapshots = true,
            Sources = true,
        });
    }

    [TearDown]
    public async Task HentikanTracingAsync()
    {
        var gagal = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        await Context.Tracing.StopAsync(new()
        {
            Path = gagal ? Path.Combine(FolderTrace, $"{NamaFileAman()}.zip") : null,
        });
    }

    protected static string FolderTrace =>
        Path.Combine(TestContext.CurrentContext.WorkDirectory, "traces");

    protected async Task LoginAsync(string username = "admin", string password = "admin123")
    {
        await LoginAsync(Page, username, password);
    }

    protected static async Task LoginAsync(IPage page, string username = "admin", string password = "admin123")
    {
        await page.GotoAsync("/Login");
        await page.GetByLabel("Username").FillAsync(username);
        await page.GetByLabel("Password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }).ClickAsync();
    }

    private static string NamaFileAman() =>
        string.Join("_", TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));
}
