using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoWeb.Pages;

public class CalculatorModel : PageModel
{
    [BindProperty]
    public double? A { get; set; }

    [BindProperty]
    public double? B { get; set; }

    [BindProperty]
    public string Operator { get; set; } = "+";

    public string? Result { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        if (A is null || B is null)
        {
            ErrorMessage = "Kedua angka wajib diisi.";
            return;
        }

        double a = A.Value, b = B.Value;

        if (Operator == "/" && b == 0)
        {
            ErrorMessage = "Tidak bisa membagi dengan nol.";
            return;
        }

        double result = Operator switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => a / b,
            _ => double.NaN
        };

        if (double.IsNaN(result))
        {
            ErrorMessage = "Operator tidak dikenal.";
            return;
        }

        Result = result.ToString(CultureInfo.InvariantCulture);
    }
}
