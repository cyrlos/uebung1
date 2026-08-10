using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var income = _context.transactions
                .Where(t => t.UserId == userId.Value && t.type == TransactionType.Income)
                .Sum(t => (decimal?)t.amount) ?? 0;

            var expense = _context.transactions
                .Where(t => t.UserId == userId.Value && t.type == TransactionType.Expense)
                .Sum(t => (decimal?)t.amount) ?? 0;

            var latestTransactions = await _context.transactions
                .Include(t => t.category)
                .Where(t => t.UserId == userId.Value)
                .OrderByDescending(t => t.date)
                .ThenByDescending(t => t.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.Income = income;
            ViewBag.Expense = expense;
            ViewBag.Balance = income - expense;

            return View(latestTransactions);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
