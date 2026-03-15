using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;

namespace QuanLyChamCong.Controllers
{
    public class SalaryController : Controller
    {
        private readonly AppDbContext _context;

        public SalaryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? month, int? year)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            int m = month ?? DateTime.Today.Month;
            int y = year  ?? DateTime.Today.Year;

            var records = _context.Attendances
                .Where(a => a.UserId == userId && a.Date.Month == m && a.Date.Year == y)
                .OrderBy(a => a.Date)
                .ToList();

            decimal totalHours = records.Sum(a => a.TotalHours) ?? 0m;
            int     workDays   = records.Count(a => a.CheckInTime != null);

            var salaryHistory = new List<object>();
            for (int i = 11; i >= 0; i--)
            {
                var d  = DateTime.Today.AddMonths(-i);
                var h  = _context.Attendances
                    .Where(a => a.UserId == userId && a.Date.Month == d.Month && a.Date.Year == d.Year)
                    .Sum(a => a.TotalHours) ?? 0m;
                salaryHistory.Add(new {
                    Label  = $"T{d.Month}/{d.Year}",
                    Hours  = h,
                    Salary = h * 30000m
                });
            }

            ViewBag.Month         = m;
            ViewBag.Year          = y;
            ViewBag.TotalHours    = totalHours;
            ViewBag.WorkDays      = workDays;
            ViewBag.Salary        = totalHours * 30000m;
            ViewBag.SalaryHistory = salaryHistory;
            ViewBag.Records       = records;

            var prevM   = m == 1 ? 12 : m - 1;
            var prevY   = m == 1 ? y - 1 : y;
            var prevHrs = _context.Attendances
                .Where(a => a.UserId == userId && a.Date.Month == prevM && a.Date.Year == prevY)
                .Sum(a => a.TotalHours) ?? 0m;
            ViewBag.PrevSalary = prevHrs * 30000m;
            ViewBag.PrevHours  = prevHrs;

            return View();
        }
    }
}
