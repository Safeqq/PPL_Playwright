# Panduan Demo Playwright (C#)

Topik kelompok: **Locators, Auto-waiting, Browser Contexts, Tracing, Cross-browser**.
Semua test ada di `tests/DemoWeb.Tests/`, satu file per topik, dan setiap langkah diberi komentar.

---

## 0. Konsep Dasar

**Playwright** = library untuk mengendalikan browser lewat kode (buka halaman, klik, ketik, lalu periksa hasilnya). Dipakai untuk *end-to-end testing*, yaitu menguji aplikasi dari sudut pandang pengguna.

```
Browser  →  satu proses browser
  └─ Context  →  seperti jendela incognito (cookie & session sendiri)
       └─ Page  →  satu tab
            └─ Locator  →  cara menemukan elemen di tab tersebut
```

Bentuk sebuah test (`DasarTests.cs`):

```csharp
[Test]
public async Task Login_DenganAkunBenar_MasukKeHalamanTodo()
{
    await Page.GotoAsync("/Login");                                           // Arrange
    await Page.GetByLabel("Username").FillAsync("admin");                     // Act
    await Page.GetByLabel("Password").FillAsync("admin123");
    await Page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }).ClickAsync();
    await Expect(Page).ToHaveURLAsync(new Regex("/Todo$"));                   // Assert
}
```

- `[Test]`: penanda test case untuk NUnit.
- `async/await`: setiap perintah dikirim ke browser lalu ditunggu jawabannya.
- `Expect(...)`: assertion Playwright yang otomatis menunggu.
- `BaseTest.cs`: kelas induk berisi BaseURL, tracing otomatis, dan helper `LoginAsync`.
- `AppServer.cs`: menyalakan web app sebelum test, mematikannya setelah selesai.

---

## 1. Daftar Test Case

| Topik | ID | Skenario | Hasil yang diharapkan |
|---|---|---|---|
| Dasar | – | Login dengan akun benar | Pindah ke `/Todo`, tampil "Halo, admin" |
| Locators | L01 | Login memakai 5 jenis locator (Role, Label, Text, CSS, TestId) | Login berhasil |
| Locators | L02 | Klik "Selesai" hanya pada todo "Belajar Playwright" (Filter + rangkai) | Hanya item itu yang dicoret |
| Locators | L03 | Klik locator yang cocok dengan banyak link | Ditolak: "strict mode violation" |
| Auto-waiting | W01 | Klik tombol yang baru aktif 2 detik kemudian | Klik berhasil tanpa Sleep, tampil "Terkirim!" |
| Auto-waiting | W02 | Muat data dari server yang lambat | `CountAsync` langsung = 0, `Expect` menunggu sampai 3 produk |
| Browser Contexts | C01 | Login di context A, buka `/Todo` di context B | B dilempar ke `/Login` |
| Browser Contexts | C02 | Login di tab 1, buka `/Todo` di tab 2 (context sama) | Tab 2 sudah login |
| Tracing | T01 | 2 + 2 diharapkan 5 (sengaja gagal, `[Explicit]`) | Gagal, file trace tersimpan |
| Cross-browser | X01 | Login di chromium, firefox, webkit, msedge | Login berhasil di semua browser |

---

## 2. Kenapa Seperti Itu?

**Locators**
- `GetByRole` jadi pilihan utama karena mencari elemen seperti pengguna melihatnya ("tombol bernama Masuk"). Kalau HTML/CSS diubah developer, test tetap jalan.
- CSS (`#password`) bisa dipakai, tapi rapuh: ganti id sedikit, test rusak, padahal tampilan sama.
- Strict mode (L03): kalau locator cocok dengan lebih dari satu elemen, Playwright menolak. Lebih baik gagal dengan pesan jelas daripada mengklik elemen yang salah.

**Auto-waiting**
- Cara lama: `Thread.Sleep(3000)`. Terlalu pendek → test gagal acak (*flaky*); terlalu panjang → test lambat.
- Playwright: aksi (`ClickAsync`) menunggu elemen siap (terlihat, stabil, enabled), dan `Expect` mencoba ulang sampai benar (maks. 5 detik).
- Jebakan (W02): `CountAsync`, `IsVisibleAsync`, `TextContentAsync` *tidak* menunggu. Untuk memeriksa hasil, selalu pakai `Expect`.

**Browser Contexts**
- Membuka browser baru itu lambat; membuat context hanya butuh milidetik, tapi isolasinya sama (cookie & session terpisah).
- Setiap test otomatis mendapat context baru → test tidak saling mempengaruhi.
- Bisa menguji beberapa pengguna sekaligus dalam satu test (C01).

**Tracing**
- Kalau test gagal di server tanpa ada yang melihat layarnya, trace menjadi "rekaman CCTV": screenshot tiap langkah, kondisi halaman sebelum/sesudah aksi, request jaringan, console, dan baris kode.
- Di `BaseTest.cs`, semua test direkam, tapi file hanya disimpan kalau test gagal.

**Cross-browser**
- Bug bisa muncul hanya di browser tertentu. Playwright mendukung Chromium (Chrome/Edge), Firefox, dan WebKit (Safari) dengan kode yang sama.
- Cara 1: `dotnet test -- Playwright.BrowserName=webkit` memindahkan **seluruh** test ke WebKit.
- Cara 2 (X01): `[TestCase]` per browser dalam satu test.

---

## 3. Naskah Demo (±15 menit)

Persiapan (sekali saja): `dotnet build`

**1. Aplikasinya (2 menit)**
```bash
dotnet run --project src/DemoWeb --urls http://localhost:5123
```
Tunjukkan http://localhost:5123: Login, Kalkulator, Todo, dan Halaman Lambat (klik "Muat Produk" supaya terlihat jedanya). Matikan dengan Ctrl+C.

**2. Dasar (1 menit)**: tampilkan `DasarTests.cs`, jelaskan Arrange-Act-Assert.
```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=Dasar
```
(Browser Edge terbuka dan setiap aksi diperlambat.)

**3. Locators (3 menit)**: tampilkan `LocatorTests.cs`.
```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=Locators
```
Bonus, Codegen (web app harus menyala di terminal lain): klik-klik di browser, kode C# muncul otomatis.
```bash
powershell -ExecutionPolicy Bypass -File tests/DemoWeb.Tests/bin/Debug/net8.0/playwright.ps1 codegen --channel msedge --target csharp-nunit http://localhost:5123
```

**4. Auto-waiting (2 menit)**: tampilkan `AutoWaitingTests.cs`. Tekankan: tidak ada Sleep.
```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=AutoWaiting
```
Di W01 browser terlihat "diam" sebentar menunggu tombol aktif, lalu mengklik.

**5. Browser Contexts (2 menit)**: tampilkan `BrowserContextTests.cs`.
```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=BrowserContexts
```
Di C01 terlihat dua jendela terbuka; jendela kedua dilempar ke halaman login.

**6. Tracing (3 menit)**
```bash
dotnet test --filter "Name=T01_SengajaGagal"
```
```bash
powershell -ExecutionPolicy Bypass -File tests/DemoWeb.Tests/bin/Debug/net8.0/playwright.ps1 show-trace tests/DemoWeb.Tests/bin/Debug/net8.0/traces/T01_SengajaGagal.zip
```
Tunjukkan di trace viewer: timeline screenshot, tab Before/After saat klik "Hitung", langkah merah (diharapkan "5", di layar "4"), lalu tab Source, Network, dan Console.

**7. Cross-browser (2 menit)**: tampilkan `CrossBrowserTests.cs`.
```bash
$env:HEADED="1"; dotnet test --filter TestCategory=CrossBrowser; Remove-Item Env:HEADED
```
Lalu seluruh test di WebKit (mesin Safari) tanpa mengubah kode:
```bash
dotnet test -- Playwright.BrowserName=webkit
```

---

## 4. Catatan Laptop Ini

Firefox dan Chromium versi terlihat (headed) tidak bisa dibuka karena sebagian file sistem Windows (WinSxS) rusak. Edge, WebKit, dan Chromium headless tetap jalan. Karena itu mode presentasi memakai Edge, dan test Firefox ditandai *Inconclusive* (bukan gagal).

Perbaikan (PowerShell sebagai Administrator, lalu restart):
```powershell
DISM /Online /Cleanup-Image /RestoreHealth
```
```powershell
sfc /scannow
```
