using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context= context;
        }
        public async Task<ActionResult> Index()
        {
            // Last 10 Days Transactions
            DateTime StartDate = DateTime.Today.AddDays(-9);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate).ToListAsync();


            // total income
            int TotalIncome = SelectedTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            CultureInfo cultureinc = CultureInfo.CreateSpecificCulture("en-IND");
            cultureinc.NumberFormat.CurrencyNegativePattern = 1;

            ViewBag.TotalIncome = String.Format(cultureinc, "{0:C0}", TotalIncome);


            // total Expense
            int TotalExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);
            CultureInfo cultureexp = CultureInfo.CreateSpecificCulture("en-IND");
            cultureexp.NumberFormat.CurrencyNegativePattern = 1;

            ViewBag.TotalExpense = String.Format(cultureexp, "{0:C0}", TotalExpense);
            
            
            //Expense Limit
            
            int ExpenselimitVal = GetExpenseLimit();
            CultureInfo cultureexplimit = CultureInfo.CreateSpecificCulture("en-IND");
            cultureexplimit.NumberFormat.CurrencyNegativePattern = 1;

            ViewBag.ExpLimit = String.Format(cultureexplimit, "{0:C0}", GetExpenseLimit());

            if (TotalExpense > ExpenselimitVal)
            {
                ViewBag.ExpenseLimitAlert = string.Format("⚠️ You Have Exceeded your Expense Limit.");
            }


            //Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culturebal = CultureInfo.CreateSpecificCulture("en-IND");
            culturebal.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culturebal, "{0:C0}", Balance);

            //Alert For Category Limit
            List<Tuple<int, int?, string?, int?>> categoryamounts = GetCategoryTotal();
            ViewBag.AlertMessage = new List<string>();
            
            foreach(Tuple<int, int?, string?, int?> categoryamount in categoryamounts)
            {
                if(categoryamount.Item1>categoryamount.Item4)
                {
                    ViewBag.AlertMessage.Add(string.Format("⚠️ {0} Category expense exceeds Category Limit.", categoryamount.Item3));
                }
            }


            if(TotalExpense>TotalIncome)
            {
                
                ViewBag.ExpenseAlert = string.Format("⚠️ Your Expense is Greater than your Income.");
            }

            //PieRadius
            ViewBag.PieRadiusData = SelectedTransactions.Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    //Y Value in Chart
                    //first because group query
                    categoryTitleWithIcon = k.First().Category.Icon+" "+ k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();


            //My Categories
            ViewBag.MyCategories = await _context.Categories.ToListAsync();

            //Recent Activity

            ViewBag.RecentActivity = await _context.Transactions.Include(i => i.Category).OrderByDescending(j => j.Date).Take(7)
                .ToListAsync();

            return View();
        }
        [NonAction]
        public List<Tuple<int, int?, string?, int?>> GetCategoryTotal()
        {
            List<Transaction> SelectedTransactions = _context.Transactions
                .Include(x => x.Category)
                .ToList();
            List<Tuple<int, int?, string?, int?>> CategoryExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense")
               .GroupBy(j => j.CategoryId)
               .Select(k => new Tuple<int, int?, string?, int?>(
                   k.Sum(j => j.Amount),
                   k.Select(j => j.CategoryId).FirstOrDefault(), 
                   k.Select(j => j.Category?.Title).FirstOrDefault(),
                   k.Select(j=>j.Category?.CategoryLimit).FirstOrDefault()

               )).ToList();

            return CategoryExpense;

            
        }
        public int GetExpenseLimit()
        {
            int Limit = _context.Expense.Select(i => i.ExpenseLimit).FirstOrDefault();
            return Limit;



        }


        // GET: /ExpenseLimit
        public IActionResult ExpenseLimit()
        {
            return View();  
            
        }

        // POST: /ExpenseLimit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpenseLimit([Bind("ExpenseLimit")] Expenses expenses)
        {
            List<Expenses> oldexpense = _context.Expense.ToList();
            if (ModelState.IsValid)
            {
                foreach (Expenses i in oldexpense)
                {
                    _context.Expense.Remove(i);
                }
                await _context.SaveChangesAsync();
                _context.Add(expenses);
                await _context.SaveChangesAsync();
                TempData["ExpenseLimitUpdate"] = "Expense Limit Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

    }
    
}
