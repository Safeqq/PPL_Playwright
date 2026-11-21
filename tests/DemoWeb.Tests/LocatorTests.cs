using Microsoft.Playwright;

namespace DemoWeb.Tests;

/// <summary>
/// LOCATORS: cara menemukan elemen di halaman.
/// Urutan yang disarankan: GetByRole → GetByLabel → GetByText → GetByTestId → CSS/XPath (terakhir).
/// Jalankan: dotnet test --filter TestCategory=Locators
/// </summary>
[Category("Locators")]
public class LocatorTests : BaseTest
{
    [Test]
    public async Task L01_JenisJenisLocator()
    {
        await Page.GotoAsync("/Login");

        // GetByRole: cari seperti pengguna melihatnya → "tombol bernama Masuk"
        var tombol = Page.GetByRole(AriaRole.Button, new() { Name = "Masuk" });

        // GetByLabel: cari input lewat teks label-nya
        var username = Page.GetByLabel("Username");

        // GetByText: cari teks yang terlihat di layar
        var petunjuk = Page.GetByText("Akun demo");

        // CSS: bisa, tapi rapuh. Kalau id="password" diganti, test rusak
        // padahal tampilan untuk pengguna sama saja.
        var password = Page.Locator("#password");

        await Expect(petunjuk).ToBeVisibleAsync();
        await username.FillAsync("admin");
        await password.FillAsync("admin123");
        await tombol.ClickAsync();

        // GetByTestId: cari lewat atribut data-testid yang dibuat khusus untuk test
        await Expect(Page.GetByTestId("current-user")).ToHaveTextAsync("Halo, admin");
    }

    [Test]
    public async Task L02_FilterDanRangkai_PilihTombolDiItemTertentu()
    {
        await LoginAsync();
        await TambahTodoAsync("Cuci piring");
        await TambahTodoAsync("Belajar Playwright");

        // Ada 2 tombol "Selesai". Kita mau yang di item "Belajar Playwright":
        // 1) Filter: ambil item yang mengandung teks tersebut
        var item = Page.GetByTestId("todo-item").Filter(new() { HasText = "Belajar Playwright" });

        // 2) Rangkai: cari tombol "Selesai" hanya di dalam item itu
        await item.GetByRole(AriaRole.Button, new() { Name = "Selesai" }).ClickAsync();

        await Expect(item).ToHaveClassAsync("done");
    }

    [Test]
    public async Task L03_StrictMode_LocatorAmbiguDitolak()
    {
        await Page.GotoAsync("/");

        // Locator ini cocok dengan SEMUA link di halaman.
        // Playwright menolak mengklik karena tidak tahu link mana yang dimaksud.
        // Ini melindungi kita dari mengklik elemen yang salah tanpa sadar.
        var error = Assert.ThrowsAsync<PlaywrightException>(
            () => Page.GetByRole(AriaRole.Link).ClickAsync());

        Assert.That(error!.Message, Does.Contain("strict mode violation"));
    }

    private async Task TambahTodoAsync(string judul)
    {
        await Page.GetByPlaceholder("Apa yang perlu dikerjakan?").FillAsync(judul);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Tambah" }).ClickAsync();
    }
}
