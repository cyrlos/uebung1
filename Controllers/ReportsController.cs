
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;

public class ReportsController : Controller
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }
  
    public IActionResult Daily()
    {
        var today = DateTime.Today;

        return RedirectToAction(nameof(Index), new
        {
            fromDate = today,
            toDate = today
        });
    }

    public IActionResult Weekly()
    {
        var today = DateTime.Today;

        // Montag der aktuellen Woche
        var monday = today.AddDays(-(int)today.DayOfWeek + 1);

        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            monday = today.AddDays(-6);
        }

        return RedirectToAction(nameof(Index), new
        {
            fromDate = monday,
            toDate = today
        });
    }

    public IActionResult Monthly()
    {
        var today = DateTime.Today;

        var firstDay = new DateTime(
            today.Year,
            today.Month,
            1);

        return RedirectToAction(nameof(Index), new
        {
            fromDate = firstDay,
            toDate = today
        });
    }

    // GET: TRANSACTIONS
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.transactions.AsQueryable();
        if (fromDate.HasValue)
        {
            query = query.Where(t => t.date >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            var nextDay = toDate.Value.Date.AddDays(1);
            query = query.Where(t => t.date < nextDay);
        }
        var transactions = await query.ToListAsync();
        ViewBag.Income = transactions.Where(t => t.type == TransactionType.Income)
                                      .Sum(t => t.amount); ViewBag.Expense = transactions
                                      .Where(t => t.type == TransactionType.Expense)
                                      .Sum(t => t.amount);
        ViewBag.Balance = ViewBag.Income - ViewBag.Expense;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        return View(transactions);
    }

    // GET: TRANSACTIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var transaction = await _context.transactions
            .FirstOrDefaultAsync(m => m.Id == id);
        if (transaction == null)
        {
            return NotFound();
        }

        return View(transaction);
    }

    // GET: TRANSACTIONS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TRANSACTIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,amount,date,description,type,categoryId,category")] Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(transaction);
    }

    // GET: TRANSACTIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var transaction = await _context.transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }
        return View(transaction);
    }

    // POST: TRANSACTIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,amount,date,description,type,categoryId,category")] Transaction transaction)
    {
        if (id != transaction.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(transaction.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(transaction);
    }

    // GET: TRANSACTIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var transaction = await _context.transactions
            .FirstOrDefaultAsync(m => m.Id == id);
        if (transaction == null)
        {
            return NotFound();
        }

        return View(transaction);
    }

    // POST: TRANSACTIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var transaction = await _context.transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.transactions.Remove(transaction);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TransactionExists(int? id)
    {
        return _context.transactions.Any(e => e.Id == id);
    }
}
