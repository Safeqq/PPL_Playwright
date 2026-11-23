using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;

namespace DemoWeb.Tests;

// File ini berisi dua class "fondasi" (tidak ada test di sini):
//   1. AppServer → berjalan SEKALI untuk semua test: menyalakan & mematikan web app
//   2. BaseTest  → berjalan untuk SETIAP test: BaseURL, tracing, helper login
// Sengaja tetap dua class karena waktu jalannya berbeda.

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

/// <summary>
/// Kelas induk untuk semua test.
///
/// Rantai pewarisan dari paket Microsoft.Playwright.NUnit:
///   PlaywrightTest  → menyediakan objek "Playwright" (pintu masuk ke semua browser)
///   BrowserTest     → menyediakan "Browser" (satu proses browser, dipakai bersama)
///   ContextTest     → menyediakan "Context" (BROWSER CONTEXT baru untuk SETIAP test)
///   PageTest        → menyediakan "Page"    (satu tab di dalam context tersebut)
///
/// Artinya, setiap test otomatis mendapat "profil browser" yang bersih:
/// cookie, session, localStorage kosong → test tidak saling mempengaruhi.
///
/// Jangan menaruh [Test] di sini: semua class turunan akan ikut mewarisi
/// dan menjalankannya berulang kali.
/// </summary>
public abstract class BaseTest : PageTest
{
    /// <summary>
    /// Pengaturan untuk browser context yang dibuat otomatis oleh PageTest.
    /// </summary>
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        // Dengan BaseURL, test cukup menulis Page.GotoAsync("/Login")
        BaseURL = AppServer.BaseUrl,
        Locale = "id-ID",
        ViewportSize = new() { Width = 1280, Height = 720 },
    };

    /// <summary>
    /// TRACING: mulai merekam sebelum setiap test.
    /// Trace berisi screenshot, snapshot DOM, request jaringan, log console,
    /// dan baris kode sumber untuk SETIAP aksi.
    /// </summary>
    [SetUp]
    public async Task MulaiTracingAsync()
    {
        await Context.Tracing.StartAsync(new()
        {
            Title = TestContext.CurrentContext.Test.Name,
            Screenshots = true, // screenshot tiap aksi (filmstrip di trace viewer)
            Snapshots = true,   // snapshot DOM sebelum/sesudah aksi (bisa di-inspect)
            Sources = true,     // tampilkan baris kode C# yang menjalankan aksi
        });
    }

    /// <summary>
    /// TRACING: hentikan rekaman setelah setiap test.
    /// File .zip hanya disimpan kalau test GAGAL, supaya folder tidak penuh.
    /// Buka dengan: playwright.ps1 show-trace traces/NamaTest.zip
    /// </summary>
    [TearDown]
    public async Task HentikanTracingAsync()
    {
        var gagal = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        await Context.Tracing.StopAsync(new()
        {
            Path = gagal ? Path.Combine(FolderTrace, $"{NamaFileAman()}.zip") : null,
        });
    }

    /// <summary>Folder penyimpanan trace: bin/Debug/net8.0/traces</summary>
    protected static string FolderTrace =>
        Path.Combine(TestContext.CurrentContext.WorkDirectory, "traces");

    /// <summary>Langkah login yang dipakai berulang oleh banyak test.</summary>
    protected async Task LoginAsync(string username = "admin", string password = "admin123")
    {
        await LoginAsync(Page, username, password);
    }

    /// <summary>Versi yang menerima page tertentu (dipakai di test multi-context).</summary>
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
