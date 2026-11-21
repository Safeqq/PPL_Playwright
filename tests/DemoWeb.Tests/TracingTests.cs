using Microsoft.Playwright;

namespace DemoWeb.Tests;

/// <summary>
/// TRACING: "rekaman CCTV" sebuah test (screenshot tiap langkah, snapshot DOM,
/// request jaringan, log console, baris kode).
/// Tracing sudah aktif untuk SEMUA test di BaseTest.cs; file disimpan kalau test gagal.
///
/// Test di bawah SENGAJA GAGAL supaya ada trace untuk dibuka. [Explicit] artinya
/// test ini tidak ikut "dotnet test" biasa, hanya jalan kalau dipanggil namanya:
///   dotnet test --filter "Name=T01_SengajaGagal"
/// Lalu buka:
///   playwright.ps1 show-trace tests/DemoWeb.Tests/bin/Debug/net8.0/traces/T01_SengajaGagal.zip
/// </summary>
[Category("Tracing")]
public class TracingTests : BaseTest
{
    [Test, Explicit("Sengaja gagal, khusus demo trace viewer")]
    public async Task T01_SengajaGagal()
    {
        await Page.GotoAsync("/Calculator");
        await Page.GetByLabel("Angka pertama").FillAsync("2");
        await Page.GetByLabel("Angka kedua").FillAsync("2");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Hitung" }).ClickAsync();

        // SALAH DISENGAJA: 2 + 2 = 4, tapi kita mengharapkan 5.
        // Di trace viewer terlihat: layar menampilkan "4", yang diharapkan "5".
        await Expect(Page.GetByTestId("calc-result")).ToHaveTextAsync("5", new() { Timeout = 2000 });
    }
}
