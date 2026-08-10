using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

public class TransactionsController : Controller
{
    private readonly AppDbContext _context;

    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Transactions
public async Task<IActionResult> Index(
    string? search,
    int? categoryId,
    decimal? minAmount,
    decimal? maxAmount,
    DateTime? fromDate,
    DateTime? toDate,
    string? sort)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var query = _context.transactions
            .Include(t => t.category)
            .Where(t => t.UserId == userId.Value)
            .AsQueryable();

        // Suche nach Beschreibung
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.description != null &&
                t.description.Contains(search));
        }

        // Filter nach Kategorie
        if (categoryId.HasValue)
        {
            query = query.Where(t =>
                t.categoryId == categoryId.Value);
        }

        // Mindestbetrag
        if (minAmount.HasValue)
        {
            query = query.Where(t =>
                t.amount >= minAmount.Value);
        }

        // Maximalbetrag
        if (maxAmount.HasValue)
        {
            query = query.Where(t =>
                t.amount <= maxAmount.Value);
        }

        // Von Datum
        if (fromDate.HasValue)
        {
            query = query.Where(t =>
                t.date >= fromDate.Value);
        }

        // Bis Datum
        if (toDate.HasValue)
        {
            query = query.Where(t =>
                t.date <= toDate.Value);
        }

        // Sortierung
        switch (sort)
        {
            case "date_asc":
                query = query.OrderBy(t => t.date);
                break;

            case "date_desc":
                query = query.OrderByDescending(t => t.date);
                break;

            case "amount_asc":
                query = query.OrderBy(t => t.amount);
                break;

            case "amount_desc":
                query = query.OrderByDescending(t => t.amount);
                break;

            default:
                query = query.OrderByDescending(t => t.date);
                break;
        }

        var transactions = await query.ToListAsync();

        ViewBag.Categories = new SelectList(
            _context.categories,
            "Id",
            "name",
            categoryId);

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.MinAmount = minAmount;
        ViewBag.MaxAmount = maxAmount;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.Sort = sort;

        return View(transactions);
    }

    // GET: /Transactions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var transaction = await _context.transactions
            .Include(t => t.category)
             .FirstOrDefaultAsync(t =>
              t.Id == id &&
              t.UserId == userId.Value);

        if (transaction == null)
        {
            return NotFound();
        }

        return View(transaction);
    }

    // GET: /Transactions/Create
    public IActionResult Create()
    {
        ViewData["categoryId"] = new SelectList(
            _context.categories,
            "Id",
            "name"
        );

        return View();
    }

    // POST: /Transactions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            transaction.UserId = userId.Value;

            _context.transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Kategorien erneut laden, falls Validierung fehlschlägt
        ViewData["categoryId"] = new SelectList(
            _context.categories,
            "Id",
            "name",
            transaction.categoryId
        );

        return View(transaction);
    }

    // GET: /Transactions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var transaction = await _context.transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId.Value);

        if (transaction == null)
        {
            return NotFound();
        }


        ViewData["categoryId"] = new SelectList(
            _context.categories,
            "Id",
            "name",
            transaction.categoryId
        );

        return View(transaction);
    }

    // POST: /Transactions/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,amount,date,description,type,categoryId")] Transaction transaction)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (id != transaction.Id)
        {
            return NotFound();
        }

        var existingTransaction = await _context.transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId.Value);

        if (existingTransaction == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existingTransaction.amount = transaction.amount;
            existingTransaction.date = transaction.date;
            existingTransaction.description = transaction.description;
            existingTransaction.type = transaction.type;
            existingTransaction.categoryId = transaction.categoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["categoryId"] = new SelectList(
            _context.categories,
            "Id",
            "name",
            transaction.categoryId
        );

        return View(transaction);
    }

    // GET: /Transactions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var transaction = await _context.transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId.Value);

        if (transaction == null)
        {
            return NotFound();
        }

        return View(transaction);
    }

    // POST: /Transactions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var transaction = await _context.transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId.Value);

        if (transaction != null)
        {
            _context.transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool TransactionExists(int id)
    {
        return _context.transactions.Any(t => t.Id == id);
    }
}