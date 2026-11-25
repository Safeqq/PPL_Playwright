using Microsoft.Playwright;

namespace DemoWeb.Tests;

// Locators: cara menemukan elemen di halaman.
[Category("Locators")]
public class LocatorTests : BaseTest
{
    [Test]
    public async Task L01_JenisJenisLocator()
    {
        await Page.GotoAsync("/Login");

        var tombol = Page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }); // paling disarankan
        var username = Page.GetByLabel("Username");
        var petunjuk = Page.GetByText("Akun demo");
        var password = Page.Locator("#password"); // CSS: rapuh kalau id diganti

        await Expect(petunjuk).ToBeVisibleAsync();
        await username.FillAsync("admin");
        await password.FillAsync("admin123");
        await tombol.ClickAsync();

        await Expect(Page.GetByTestId("current-user")).ToHaveTextAsync("Halo, admin");
    }

    [Test]
    public async Task L02_FilterDanRangkai_PilihTombolDiItemTertentu()
    {
        await LoginAsync();
        await TambahTodoAsync("Cuci piring");
        await TambahTodoAsync("Belajar Playwright");

        // Ada 2 tombol "Selesai": filter item-nya, lalu cari tombol di dalamnya
        var item = Page.GetByTestId("todo-item").Filter(new() { HasText = "Belajar Playwright" });
        await item.GetByRole(AriaRole.Button, new() { Name = "Selesai" }).ClickAsync();

        await Expect(item).ToHaveClassAsync("done");
    }

    [Test]
    public async Task L03_StrictMode_LocatorAmbiguDitolak()
    {
        await Page.GotoAsync("/");

        // Cocok dengan banyak link, jadi Playwright menolak mengklik
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
