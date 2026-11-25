using Microsoft.Playwright;

namespace DemoWeb.Tests;

// Auto-waiting: Playwright menunggu elemen siap tanpa Thread.Sleep.
// Memakai halaman /Lambat yang sengaja dibuat lambat (±2 detik).
[Category("AutoWaiting")]
public class AutoWaitingTests : BaseTest
{
    [Test]
    public async Task W01_KlikTombolYangBaruAktif2DetikKemudian()
    {
        await Page.GotoAsync("/Lambat");
        var kirim = Page.GetByRole(AriaRole.Button, new() { Name = "Kirim" });

        await Expect(kirim).ToBeDisabledAsync();
        await kirim.ClickAsync(); // menunggu sampai tombol aktif, baru klik

        await Expect(Page.GetByTestId("status-kirim")).ToHaveTextAsync("Terkirim!");
    }

    [Test]
    public async Task W02_DataDariServerYangLambat()
    {
        await Page.GotoAsync("/Lambat");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Muat Produk" }).ClickAsync();

        var produk = Page.GetByTestId("produk-list").GetByRole(AriaRole.Listitem);

        // CountAsync tidak menunggu → data belum datang
        Assert.That(await produk.CountAsync(), Is.EqualTo(0));

        // Expect mencoba ulang sampai data muncul
        await Expect(produk).ToHaveTextAsync(new[] { "Laptop", "Mouse", "Keyboard" });
    }
}
