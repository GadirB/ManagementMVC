using ManagementMVC.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Globalization;

namespace ManagementMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly FinanceDbContext _context;
        public DashboardController(FinanceDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions.Include(x => x.Category)
                                                            .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                                                            .ToListAsync();

            decimal TotalIncome = SelectedTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            decimal TotalExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            decimal Balance = TotalIncome - TotalExpense;
            //CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            //culture.NumberFormat.CurrencyNegativePattern = 1;
            //ViewBag.Balance = String.Format(culture, "{0:C0}", Balance);
            ViewBag.Balance = Balance.ToString("C0");

            ViewBag.DoughnutChartData = SelectedTransactions.Where(i => i.Category.Type == "Expense")
                                                            .GroupBy(j => j.Category.CategoryId)
                                                            .Select(k => new
                                                            {
                                                                categoryTitleWithIcon = k.First().Category.Icon + " " +                                                             k.First().Category.Title,
                                                                amount = k.Sum(j => j.Amount),
                                                                formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                                                            })
                                                            .OrderByDescending(l => l.amount)
                                                            .ToList();


            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    Income = k.Sum(l => l.Amount)
                })
                .ToList();

            List<SplineChartData> ExpenseSummary = SelectedTransactions
              .Where(i => i.Category.Type == "Expense")
              .GroupBy(j => j.Date)
              .Select(k => new SplineChartData()
              {
                  Day = k.First().Date.ToString("dd-MMM"),
                  Expense = k.Sum(l => l.Amount)
              })
              .ToList();

            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from Day in Last7Days
                                      join Income in IncomeSummary on Day equals Income.Day into dayIncomeJoined
                                      from Income in dayIncomeJoined.DefaultIfEmpty()
                                      join Expense in ExpenseSummary on Day equals Expense.Day into expenseJoined
                                      from Expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          Day = Day,
                                          Income = Income == null ? 0 : Income.Income,
                                          Expense = Expense == null ? 0 : Expense.Expense,
                                      };

            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }

    public class SplineChartData
    {
        public string Day;
        public decimal Income;
        public decimal Expense;
    }
}
