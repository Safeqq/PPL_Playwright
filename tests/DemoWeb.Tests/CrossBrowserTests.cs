using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace DemoWeb.Tests;

/// <summary>
/// CROSS-BROWSER: satu kode test, beberapa browser.
///   chromium → Chrome/Edge, firefox → Firefox, webkit → Safari
///
/// Cara 1: seluruh test dipindah ke browser lain tanpa mengubah kode:
///   dotnet test -- Playwright.BrowserName=webkit
/// Cara 2 (di bawah): satu test dijalankan di tiap browser lewat [TestCase].
///
/// Jalankan: dotnet test --filter TestCategory=CrossBrowser
/// Browser terlihat: $env:HEADED="1"; dotnet test --filter TestCategory=CrossBrowser
/// </summary>
[Category("CrossBrowser")]
public class CrossBrowserTests : PlaywrightTest
{
    [TestCase("chromium")]
    [TestCase("firefox")]
    [TestCase("webkit")]
    [TestCase("msedge")]
    public async Task X01_LoginDiSemuaBrowser(string namaBrowser)
    {
        // 1. Buka browser sesuai parameter
        await using var browser = await BukaBrowserAsync(namaBrowser);
        var page = await browser.NewPageAsync(new() { BaseURL = AppServer.BaseUrl });

        // 2. Langkah test sama persis untuk semua browser
        await page.GotoAsync("/Login");
        await page.GetByLabel("Username").FillAsync("admin");
        await page.GetByLabel("Password").FillAsync("admin123");
        await page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/Todo$"));

        // 3. Cetak versi browser sebagai bukti browsernya memang berbeda
        TestContext.Out.WriteLine($"{namaBrowser} versi {browser.Version}");
    }

    private async Task<IBrowser> BukaBrowserAsync(string namaBrowser)
    {
        var terlihat = Environment.GetEnvironmentVariable("HEADED") == "1";
        var opsi = new BrowserTypeLaunchOptions { Headless = !terlihat, SlowMo = terlihat ? 300 : 0 };

        try
        {
            if (namaBrowser == "msedge")
            {
                // Edge memakai mesin chromium, dengan Edge yang terpasang di Windows
                opsi.Channel = "msedge";
                return await Playwright.Chromium.LaunchAsync(opsi);
            }
            return await Playwright[namaBrowser].LaunchAsync(opsi);
        }
        catch (PlaywrightException e) when (e.Message.Contains("spawn UNKNOWN") || e.Message.Contains("Executable doesn't exist"))
        {
            // Browser tidak bisa dijalankan di komputer ini → "Inconclusive", bukan gagal
            Assert.Inconclusive($"{namaBrowser} tidak bisa dijalankan di komputer ini");
            throw;
        }
    }
}
