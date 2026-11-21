using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace DemoWeb.Tests;

/// <summary>
/// BROWSER CONTEXTS
///   Browser → satu proses browser (lambat dibuka)
///   Context → seperti jendela incognito: cookie & session sendiri (cepat dibuat)
///   Page    → satu tab di dalam context
/// Setiap test otomatis mendapat context baru, jadi test tidak saling mempengaruhi.
/// Jalankan: dotnet test --filter TestCategory=BrowserContexts
/// </summary>
[Category("BrowserContexts")]
public class BrowserContextTests : BaseTest
{
    [Test]
    public async Task C01_DuaContext_TidakBerbagiLogin()
    {
        // Context A (bawaan test): login
        await LoginAsync(Page);

        // Context B: "jendela incognito" baru di browser yang sama
        var contextB = await NewContext(ContextOptions());
        var pageB = await contextB.NewPageAsync();

        // B tidak punya cookie login milik A → dilempar ke halaman login
        await pageB.GotoAsync("/Todo");
        await Expect(pageB).ToHaveURLAsync(new Regex("/Login$"));
    }

    [Test]
    public async Task C02_SatuContextDuaTab_BerbagiLogin()
    {
        // Login di tab pertama
        await LoginAsync(Page);

        // Tab kedua dibuka di context yang SAMA
        var tabKedua = await Context.NewPageAsync();
        await tabKedua.GotoAsync("/Todo");

        // Cookie dipakai bersama → tab kedua sudah login
        await Expect(tabKedua.GetByTestId("current-user")).ToHaveTextAsync("Halo, admin");
    }
}
