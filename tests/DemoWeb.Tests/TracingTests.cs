using Microsoft.Playwright;

namespace DemoWeb.Tests;

// Tracing: perekaman diatur di BaseTest. Test ini sengaja gagal supaya ada trace.
// Jalankan: dotnet test --filter "Name=T01_SengajaGagal"
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

        // Sengaja salah: 2 + 2 = 4, bukan 5
        await Expect(Page.GetByTestId("calc-result")).ToHaveTextAsync("5", new() { Timeout = 2000 });
    }
}
