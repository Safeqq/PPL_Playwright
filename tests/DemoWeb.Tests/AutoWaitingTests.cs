using Microsoft.Playwright;

namespace DemoWeb.Tests;

/// <summary>
/// AUTO-WAITING: Playwright otomatis menunggu elemen siap, jadi tidak perlu Thread.Sleep.
///   - Aksi (Click, Fill) menunggu elemen terlihat, stabil, dan enabled.
///   - Expect(...) memeriksa berulang sampai benar (batas default 5 detik).
/// Kedua test memakai halaman /Lambat yang sengaja dibuat lambat.
/// Jalankan: dotnet test --filter TestCategory=AutoWaiting
/// </summary>
[Category("AutoWaiting")]
public class AutoWaitingTests : BaseTest
{
    [Test]
    public async Task W01_KlikTombolYangBaruAktif2DetikKemudian()
    {
        await Page.GotoAsync("/Lambat");
        var kirim = Page.GetByRole(AriaRole.Button, new() { Name = "Kirim" });

        // Saat halaman baru dibuka, tombol masih disabled
        await Expect(kirim).ToBeDisabledAsync();

        // ClickAsync menunggu sendiri sampai tombol aktif (±2 detik), baru mengklik
        await kirim.ClickAsync();

        await Expect(Page.GetByTestId("status-kirim")).ToHaveTextAsync("Terkirim!");
    }

    [Test]
    public async Task W02_DataDariServerYangLambat()
    {
        await Page.GotoAsync("/Lambat");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Muat Produk" }).ClickAsync();

        var produk = Page.GetByTestId("produk-list").GetByRole(AriaRole.Listitem);

        // JEBAKAN: CountAsync tidak menunggu, hanya melihat kondisi saat ini.
        // Data belum datang dari server, jadi hasilnya 0.
        Assert.That(await produk.CountAsync(), Is.EqualTo(0));

        // Cara yang benar: Expect mencoba ulang sampai data muncul
        await Expect(produk).ToHaveTextAsync(new[] { "Laptop", "Mouse", "Keyboard" });
    }
}
