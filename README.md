# Demo Playwright dengan C#

Aplikasi web sederhana (ASP.NET Core Razor Pages, .NET 8) beserta test end-to-end menggunakan **Playwright for .NET** + NUnit.

## Struktur

```
src/DemoWeb/            Aplikasi web yang diuji
  Pages/Index           Beranda
  Pages/Login           Login (akun demo: admin / admin123)
  Pages/Calculator      Kalkulator + - × ÷
  Pages/Todo            Todo list (wajib login, data disimpan di session)
  Pages/Lambat          Elemen yang siap terlambat (demo auto-waiting)
tests/DemoWeb.Tests/    Test Playwright
  AppServer.cs          Menjalankan DemoWeb otomatis sebelum test
  BaseTest.cs           BaseURL + rekam trace saat test gagal
  DasarTests.cs          1 test: bentuk dasar sebuah test
  LocatorTests.cs        3 test: jenis locator, filter, strict mode
  AutoWaitingTests.cs    2 test: menunggu otomatis tanpa Sleep
  BrowserContextTests.cs 2 test: isolasi antar context, tab berbagi login
  TracingTests.cs        1 test: sengaja gagal untuk demo trace viewer
  CrossBrowserTests.cs   1 test x 4 browser: chromium, firefox, webkit, msedge
  demo.runsettings       Mode presentasi (browser terlihat + slow motion)
```

Penjelasan lengkap, daftar test case, dan naskah demo ada di **[PANDUAN-DEMO.md](PANDUAN-DEMO.md)**.

Jalankan satu topik saja: `dotnet test --filter TestCategory=Locators`

## Persiapan (sekali saja)

```bash
dotnet build
```

```bash
powershell -ExecutionPolicy Bypass -File tests/DemoWeb.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
```

## Menjalankan

Menjalankan aplikasi web secara manual (buka http://localhost:5123):

```bash
dotnet run --project src/DemoWeb --urls http://localhost:5123
```

Menjalankan semua test (headless, cepat). Aplikasi web otomatis dinyalakan oleh test:

```bash
dotnet test
```

Mode presentasi: browser Edge terlihat dan tiap aksi diperlambat 500 ms:

```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings
```
Hanya satu topik:

```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=Locators
```

Kategori: `Dasar`, `Locators`, `AutoWaiting`, `BrowserContexts`, `Tracing`, `CrossBrowser`.

Codegen, trace viewer, naskah demo, dan catatan masalah browser di laptop ini ada di **[PANDUAN-DEMO.md](PANDUAN-DEMO.md)**.
