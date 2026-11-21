using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace DemoWeb.Tests;

/// <summary>
/// DASAR: bentuk paling sederhana sebuah test Playwright.
/// Jalankan: dotnet test --filter TestCategory=Dasar
/// </summary>
[Category("Dasar")]
public class DasarTests : BaseTest   // BaseTest → PageTest: otomatis punya "Page" (satu tab browser)
{
    [Test]   // penanda test case untuk NUnit
    public async Task Login_DenganAkunBenar_MasukKeHalamanTodo()
    {
        // Arrange: buka halaman login ("/Login" digabung dengan BaseURL)
        await Page.GotoAsync("/Login");

        // Act: isi form lalu klik tombol
        await Page.GetByLabel("Username").FillAsync("admin");
        await Page.GetByLabel("Password").FillAsync("admin123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }).ClickAsync();

        // Assert: pindah ke /Todo dan nama user tampil
        await Expect(Page).ToHaveURLAsync(new Regex("/Todo$"));
        await Expect(Page.GetByTestId("current-user")).ToHaveTextAsync("Halo, admin");
    }
}
