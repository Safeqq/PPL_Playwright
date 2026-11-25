using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace DemoWeb.Tests;

// Cross-browser: test yang sama dijalankan di beberapa browser.
// Browser terlihat: $env:HEADED="1"; dotnet test --filter TestCategory=CrossBrowser
[Category("CrossBrowser")]
public class CrossBrowserTests : PlaywrightTest
{
    [TestCase("chromium")]
    [TestCase("firefox")]
    [TestCase("webkit")]
    [TestCase("msedge")]
    public async Task X01_LoginDiSemuaBrowser(string namaBrowser)
    {
        await using var browser = await BukaBrowserAsync(namaBrowser);
        var page = await browser.NewPageAsync(new() { BaseURL = AppServer.BaseUrl });

        await page.GotoAsync("/Login");
        await page.GetByLabel("Username").FillAsync("admin");
        await page.GetByLabel("Password").FillAsync("admin123");
        await page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/Todo$"));

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
                opsi.Channel = "msedge"; // Edge = mesin chromium
                return await Playwright.Chromium.LaunchAsync(opsi);
            }
            return await Playwright[namaBrowser].LaunchAsync(opsi);
        }
        catch (PlaywrightException e) when (e.Message.Contains("spawn UNKNOWN") || e.Message.Contains("Executable doesn't exist"))
        {
            // Masalah komputer, bukan aplikasi → Inconclusive
            Assert.Inconclusive($"{namaBrowser} tidak bisa dijalankan di komputer ini");
            throw;
        }
    }
}
