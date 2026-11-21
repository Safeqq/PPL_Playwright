using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoWeb.Pages;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool Done { get; set; }
}

public class TodoModel : PageModel
{
    private const string SessionKey = "Todos";

    [BindProperty]
    public string NewTitle { get; set; } = "";

    public List<TodoItem> Items { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    private bool IsLoggedIn => HttpContext.Session.GetString(LoginModel.SessionKey) is not null;

    public IActionResult OnGet()
    {
        if (!IsLoggedIn) return RedirectToPage("/Login");
        Items = Load();
        return Page();
    }

    public IActionResult OnPostAdd()
    {
        if (!IsLoggedIn) return RedirectToPage("/Login");
        Items = Load();

        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            ErrorMessage = "Judul tugas tidak boleh kosong.";
            return Page();
        }

        var nextId = Items.Count == 0 ? 1 : Items.Max(i => i.Id) + 1;
        Items.Add(new TodoItem { Id = nextId, Title = NewTitle.Trim() });
        Save(Items);
        return RedirectToPage();
    }

    public IActionResult OnPostToggle(int id)
    {
        if (!IsLoggedIn) return RedirectToPage("/Login");
        Items = Load();
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is not null)
        {
            item.Done = !item.Done;
            Save(Items);
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        if (!IsLoggedIn) return RedirectToPage("/Login");
        Items = Load();
        Items.RemoveAll(i => i.Id == id);
        Save(Items);
        return RedirectToPage();
    }

    private List<TodoItem> Load()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        return json is null ? new() : JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new();
    }

    private void Save(List<TodoItem> items) =>
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(items));
}
