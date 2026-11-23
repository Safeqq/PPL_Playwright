# Belajar Playwright dengan C#

Aplikasi web sederhana (ASP.NET Core Razor Pages, .NET 8) beserta test end-to-end menggunakan **Playwright for .NET** + NUnit.

Topik yang dibahas: **Locators, Auto-waiting, Browser Contexts, Tracing, Cross-browser**.

---

## 1. Apa itu Playwright?

Biasanya web dites secara manual: buka browser, ketik username, klik tombol, lalu lihat apakah hasilnya benar. Playwright melakukan hal yang sama, tapi **lewat kode**. Langkah-langkahnya ditulis dalam C#, lalu Playwright menjalankan browser sungguhan dan mengikuti langkah itu secara otomatis.

Jenis pengujian ini disebut **End-to-End (E2E) testing**: aplikasi diuji dari sisi pengguna, dari tampilan sampai server, tanpa melihat isi kode aplikasinya.

### Cara kerjanya

```
Kode test C#  ──►  Library Playwright (.NET)  ──►  Driver Playwright  ──►  Browser
(NUnit)            (paket NuGet)                  (proses kecil di          (Chromium /
                                                   belakang layar)          Firefox / WebKit)
```

1. Kode menulis `await Page.GetByLabel("Username").FillAsync("admin");`
2. Library Playwright mengubahnya menjadi perintah dan mengirimnya ke *driver*.
3. Driver menyuruh browser: "cari input berlabel Username, lalu ketik admin".
4. Browser mengerjakannya, lalu jawabannya dikirim balik ke kode.

Karena ada proses kirim lalu tunggu jawaban, semua perintah Playwright memakai **`async`/`await`**: kirim perintah, tunggu selesai, baru lanjut ke baris berikutnya.

### Empat objek utama

```
Browser   → aplikasi browsernya (misal Chrome). Lambat dibuka, jadi dibuka sekali saja.
 └ Context → seperti jendela incognito: punya cookie & login sendiri. Cepat dibuat.
    └ Page  → satu tab di dalam jendela itu. Hampir semua perintah lewat sini.
       └ Locator → "alamat" sebuah elemen di halaman (tombol, input, teks).
```

### Tiga jenis perintah yang paling sering dipakai

| Jenis | Contoh | Fungsinya |
|---|---|---|
| **Navigasi** | `Page.GotoAsync("/Login")` | Membuka halaman |
| **Aksi** | `.ClickAsync()`, `.FillAsync("teks")`, `.SelectOptionAsync("+")` | Klik, ketik, pilih |
| **Pemeriksaan** | `Expect(...).ToHaveTextAsync("Halo")` | Mengecek apakah hasilnya benar. Kalau salah, test gagal |

---

## 2. Persiapan

Butuh **.NET SDK 8** atau lebih baru.

1. Build project (sekaligus mengunduh paket NuGet):
   ```bash
   dotnet build
   ```
2. Unduh browser yang dipakai Playwright:
   ```bash
   powershell -ExecutionPolicy Bypass -File tests/DemoWeb.Tests/bin/Debug/net8.0/playwright.ps1 install chromium firefox webkit
   ```

---

## 3. Cara Menjalankan

Menjalankan web app secara manual, lalu buka http://localhost:5123 (akun: `admin` / `admin123`):
```bash
dotnet run --project src/DemoWeb --urls http://localhost:5123
```

Menjalankan semua test (browser tidak terlihat, cepat). Web app otomatis dinyalakan oleh test:
```bash
dotnet test
```

Menjalankan dengan browser **terlihat** dan setiap aksi diperlambat 500 ms:
```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings
```

Menjalankan satu topik saja (kategori: `Dasar`, `Locators`, `AutoWaiting`, `BrowserContexts`, `Tracing`, `CrossBrowser`):
```bash
dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=Locators
```

### Apa yang terjadi saat `dotnet test` dijalankan

1. .NET mem-*build* web app dan project test.
2. **NUnit** mencari semua method bertanda `[Test]`.
3. Sebelum test pertama, `AppServer.cs` menyalakan web app di `http://localhost:5123`.
4. Untuk **setiap** test:
   - Playwright membuat context dan tab baru yang bersih.
   - `BaseTest.cs` mulai merekam trace.
   - Isi test dijalankan.
   - Rekaman trace dihentikan (disimpan kalau test gagal), lalu context ditutup.
5. Setelah semua test selesai, web app dimatikan dan hasilnya muncul di terminal.

---

## 4. Fungsi Tiap File

### Web app yang diuji (`src/DemoWeb/`)

Satu halaman Razor terdiri dari file `.cshtml` (tampilan HTML) dan `.cshtml.cs` (logika C#).

| File | Fungsi |
|---|---|
| `DemoWeb.csproj` | Konfigurasi project web (.NET 8) |
| `Program.cs` | Titik awal aplikasi: mengaktifkan Razor Pages dan *session* (menyimpan status login), plus endpoint `/api/produk` yang sengaja diperlambat 2 detik |
| `Pages/Shared/_Layout.cshtml` | Kerangka semua halaman: menu atas dan footer |
| `Pages/_ViewStart.cshtml`, `_ViewImports.cshtml` | Pengaturan bawaan Razor |
| `Pages/Index.cshtml` | Beranda: 4 kartu menuju fitur-fitur |
| `Pages/Login.cshtml` + `.cs` | Form login. Kalau berhasil, nama user disimpan di session lalu diarahkan ke `/Todo` |
| `Pages/Calculator.cshtml` + `.cs` | Kalkulator + − × ÷ |
| `Pages/Todo.cshtml` + `.cs` | Daftar tugas (tambah, selesai, hapus). Wajib login; data disimpan di session |
| `Pages/Lambat.cshtml` | Halaman yang sengaja lambat: tombol "Kirim" baru aktif setelah 2 detik, data produk baru datang 2 detik setelah diklik |
| `wwwroot/css/site.css` | Tampilan |

Atribut seperti `data-testid="current-user"` dan `<label for="username">` di HTML sengaja ditambahkan supaya elemen **mudah ditemukan oleh test**.

### Project test (`tests/DemoWeb.Tests/`)

| File | Fungsi |
|---|---|
| `DemoWeb.Tests.csproj` | Paket yang dipakai: **NUnit** (framework test) dan **Microsoft.Playwright.NUnit** (Playwright + integrasinya dengan NUnit) |
| `AppServer.cs` | Menyalakan web app sebelum semua test dan mematikannya di akhir |
| `BaseTest.cs` | Kelas induk semua test: `BaseURL`, rekam trace (disimpan kalau gagal), dan helper `LoginAsync()` |
| `DasarTests.cs` | 1 test login: bentuk paling dasar sebuah test |
| `LocatorTests.cs` | 3 test: jenis-jenis locator, filter + rangkai, strict mode |
| `AutoWaitingTests.cs` | 2 test di halaman `/Lambat`: menunggu otomatis tanpa `Sleep` |
| `BrowserContextTests.cs` | 2 test: dua context tidak berbagi login, dua tab dalam satu context berbagi login |
| `TracingTests.cs` | 1 test yang sengaja gagal untuk menghasilkan file trace |
| `CrossBrowserTests.cs` | 1 test login yang dijalankan di chromium, firefox, webkit, dan msedge |
| `demo.runsettings` | Pengaturan browser terlihat + diperlambat, memakai Edge |

### Hubungan antar kelas test

```
PageTest (dari Playwright)      → otomatis menyediakan Browser, Context, Page
  └ BaseTest (buatan sendiri)   → + BaseURL, tracing, LoginAsync
      ├ DasarTests
      ├ LocatorTests
      ├ AutoWaitingTests
      ├ BrowserContextTests
      └ TracingTests

PlaywrightTest (dari Playwright) → hanya menyediakan "Playwright", tanpa Page otomatis
  └ CrossBrowserTests            → membuka browser sendiri karena browsernya berganti-ganti
```

---

## 5. Membedah Satu Test

Isi `DasarTests.cs`:

```csharp
[Category("Dasar")]                       // ①
public class DasarTests : BaseTest        // ②
{
    [Test]                                // ③
    public async Task Login_DenganAkunBenar_MasukKeHalamanTodo()   // ④
    {
        await Page.GotoAsync("/Login");                            // ⑤

        await Page.GetByLabel("Username").FillAsync("admin");      // ⑥
        await Page.GetByLabel("Password").FillAsync("admin123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Masuk" }).ClickAsync();  // ⑦

        await Expect(Page).ToHaveURLAsync(new Regex("/Todo$"));    // ⑧
        await Expect(Page.GetByTestId("current-user")).ToHaveTextAsync("Halo, admin");  // ⑨
    }
}
```

| No | Penjelasan |
|---|---|
| ① | Label kategori, supaya bisa dijalankan sendiri dengan `--filter TestCategory=Dasar` |
| ② | Mewarisi `BaseTest`, jadi `Page` sudah tersedia |
| ③ | Memberi tahu NUnit bahwa method ini adalah test case |
| ④ | Nama test menjelaskan **apa yang dites** dan **hasil yang diharapkan** |
| ⑤ | Buka `http://localhost:5123/Login` (BaseURL + "/Login") |
| ⑥ | `GetByLabel("Username")` adalah **locator**: "cari input berlabel Username". `FillAsync` mengetik ke input itu |
| ⑦ | "Cari **tombol** bertuliskan Masuk", lalu klik. Playwright otomatis **menunggu** tombol siap sebelum mengklik |
| ⑧ | **Pemeriksaan**: URL harus berakhiran `/Todo`. Kalau belum, dicoba ulang sampai 5 detik, baru dinyatakan gagal |
| ⑨ | Teks di elemen `data-testid="current-user"` harus "Halo, admin" |

Pola ⑤ → ⑥⑦ → ⑧⑨ disebut **Arrange – Act – Assert**: siapkan kondisi, lakukan aksi, periksa hasil. Semua test di project ini mengikuti pola tersebut.

---

## 6. Lima Topik

### Locators (`LocatorTests.cs`)
Cara menemukan elemen. Urutan yang disarankan: `GetByRole` → `GetByLabel` → `GetByText` → `GetByTestId` → CSS/XPath.
- `GetByRole` mencari elemen seperti pengguna melihatnya ("tombol bernama Masuk"), jadi test tidak rusak kalau HTML/CSS diubah.
- CSS (`#password`) bisa dipakai, tapi rapuh: ganti id sedikit, test rusak padahal tampilan sama.
- **Strict mode**: kalau locator cocok dengan lebih dari satu elemen, Playwright menolak bertindak, supaya tidak mengklik elemen yang salah.

### Auto-waiting (`AutoWaitingTests.cs`)
Playwright menunggu sendiri, jadi tidak perlu `Thread.Sleep`.
- Aksi (`ClickAsync`, `FillAsync`) menunggu elemen siap: terlihat, stabil, dan enabled.
- `Expect(...)` memeriksa berulang sampai kondisinya benar (maksimal 5 detik).
- Jebakan: `CountAsync`, `IsVisibleAsync`, `TextContentAsync` **tidak** menunggu. Untuk memeriksa hasil, selalu pakai `Expect`.

### Browser Contexts (`BrowserContextTests.cs`)
Context itu seperti jendela incognito: cookie dan login terpisah, tapi tetap memakai proses browser yang sama.
- Membuat context hanya butuh milidetik, jauh lebih cepat daripada membuka browser baru.
- Setiap test otomatis mendapat context baru, jadi test tidak saling mempengaruhi.
- Beberapa pengguna bisa diuji sekaligus dalam satu test.

### Tracing (`BaseTest.cs` + `TracingTests.cs`)
Trace adalah rekaman lengkap jalannya test: screenshot tiap langkah, kondisi halaman sebelum/sesudah aksi, request jaringan, console, dan baris kode.
- Semua test direkam, tapi file trace hanya disimpan kalau test gagal, di `tests/DemoWeb.Tests/bin/Debug/net8.0/traces/`.
- Membuat trace dari test yang sengaja gagal:
  ```bash
  dotnet test --filter "Name=T01_SengajaGagal"
  ```
- Membuka trace di trace viewer:
  ```bash
  powershell -ExecutionPolicy Bypass -File tests/DemoWeb.Tests/bin/Debug/net8.0/playwright.ps1 show-trace tests/DemoWeb.Tests/bin/Debug/net8.0/traces/T01_SengajaGagal.zip
  ```

### Cross-browser (`CrossBrowserTests.cs`)
Kode test yang sama bisa dijalankan di Chromium (Chrome/Edge), Firefox, dan WebKit (mesin Safari).
- Cara 1, memindahkan **seluruh** test ke browser lain tanpa mengubah kode:
  ```bash
  dotnet test -- Playwright.BrowserName=webkit
  ```
- Cara 2, satu test dijalankan di tiap browser lewat `[TestCase]` (lihat isi file). Agar browsernya terlihat:
  ```bash
  $env:HEADED="1"; dotnet test --filter TestCategory=CrossBrowser; Remove-Item Env:HEADED
  ```

### Bonus: Codegen
Playwright bisa membuatkan kode test dengan merekam klik di browser. Nyalakan web app dulu, lalu:
```bash
powershell -ExecutionPolicy Bypass -File tests/DemoWeb.Tests/bin/Debug/net8.0/playwright.ps1 codegen --channel msedge --target csharp-nunit http://localhost:5123
```

---

## 7. Urutan Belajar yang Disarankan

1. Jalankan web app dan coba klik-klik sendiri, supaya tahu apa yang akan dites.
2. Baca `DasarTests.cs`, lalu jalankan dengan browser terlihat:
   ```bash
   dotnet test -s tests/DemoWeb.Tests/demo.runsettings --filter TestCategory=Dasar
   ```
3. Rusak test-nya dengan sengaja (misal ganti `"Halo, admin"` jadi `"Halo, budi"`), jalankan lagi, lalu baca pesan error-nya.
4. Lanjut berurutan: Locators → Auto-waiting → Browser Contexts → Tracing → Cross-browser. Setiap file sudah diberi komentar di setiap langkah.

---

## Catatan: Masalah Browser di Laptop Ini

Firefox dan Chromium dalam mode terlihat gagal dibuka (`spawn UNKNOWN`) karena sebagian file sistem Windows (WinSxS) rusak. Edge, WebKit, dan Chromium headless tetap jalan. Karena itu `demo.runsettings` dan codegen memakai Edge, dan test Firefox ditandai *Inconclusive* (bukan gagal).

Perbaikan: buka PowerShell sebagai Administrator, jalankan dua perintah berikut, lalu restart.
```powershell
DISM /Online /Cleanup-Image /RestoreHealth
```
```powershell
sfc /scannow
```
