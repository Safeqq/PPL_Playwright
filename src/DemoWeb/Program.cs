using System.Globalization;

// Pakai format angka invariant (titik desimal) apa pun locale Windows-nya
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseSession();
app.MapRazorPages();

// Endpoint yang sengaja diperlambat, dipakai halaman /Lambat untuk demo auto-waiting
app.MapGet("/api/produk", async (int? jeda) =>
{
    await Task.Delay(jeda ?? 2000);
    return new[] { "Laptop", "Mouse", "Keyboard" };
});

app.Run();
