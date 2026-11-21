using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;

namespace DemoWeb.Tests;

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
