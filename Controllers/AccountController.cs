using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        // Prüfen, ob Benutzername bereits existiert
        var exists = _context.users
            .Any(u => u.Username == user.Username);

        if (exists)
        {
            ModelState.AddModelError(
                "Username",
                "Dieser Benutzername existiert bereits.");

            return View(user);
        }

        _context.users.Add(user);

        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
        var user = _context.users
            .FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password);

        if (user == null)
        {
            ViewBag.Error = "Benutzername oder Passwort falsch.";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);

        return RedirectToAction("Index", "");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Account");
    }
}