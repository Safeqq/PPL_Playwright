using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoWeb.Pages;

public class LoginModel : PageModel
{
    public const string SessionKey = "User";

    // Akun demo (hardcoded, hanya untuk keperluan presentasi)
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin123";

    [BindProperty]
    public string Username { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username dan password wajib diisi.";
            return Page();
        }

        if (Username != DemoUsername || Password != DemoPassword)
        {
            ErrorMessage = "Username atau password salah.";
            return Page();
        }

        HttpContext.Session.SetString(SessionKey, Username);
        return RedirectToPage("/Todo");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }
}
