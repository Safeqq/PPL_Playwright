using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace DemoWeb.Tests;

// Browser contexts: seperti jendela incognito, cookie & login terpisah.
[Category("BrowserContexts")]
public class BrowserContextTests : BaseTest
{
    [Test]
    public async Task C01_DuaContext_TidakBerbagiLogin()
    {
        await LoginAsync(Page); // context A

        var contextB = await NewContext(ContextOptions());
        var pageB = await contextB.NewPageAsync();

        // B tidak punya login A → dilempar ke halaman login
        await pageB.GotoAsync("/Todo");
        await Expect(pageB).ToHaveURLAsync(new Regex("/Login$"));
    }

    [Test]
    public async Task C02_SatuContextDuaTab_BerbagiLogin()
    {
        await LoginAsync(Page);

        var tabKedua = await Context.NewPageAsync(); // context yang sama
        await tabKedua.GotoAsync("/Todo");

        await Expect(tabKedua.GetByTestId("current-user")).ToHaveTextAsync("Halo, admin");
    }
}
