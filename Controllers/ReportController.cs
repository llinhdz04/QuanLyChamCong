using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChamCong.Data;
using QuanLyChamCong.Filters;
using QuanLyChamCong.ViewModels;

namespace QuanLyChamCong.Controllers
{
    [AdminAuthorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var report = _context.Attendances
                .GroupBy(x => x.UserId)
                .Select(g => new ReportVM
                {
                    UserId = g.Key,
                    TotalHours = g.Sum(x => x.TotalHours) ?? 0m,
                    Salary = (g.Sum(x => x.TotalHours) ?? 0m) * 30000m
                })
                .ToList();

            return View(report);
        }

        public IActionResult Monthly(int? month, int? year)
        {
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var report = _context.Attendances
                .Where(x => x.Date.Month == m && x.Date.Year == y)
                .GroupBy(x => x.UserId)
                .Select(g => new ReportVM
                {
                    UserId = g.Key,
                    TotalHours = g.Sum(x => x.TotalHours) ?? 0m,
                    Salary = (g.Sum(x => x.TotalHours) ?? 0m) * 30000m
                })
                .ToList();

            ViewBag.Month = m;
            ViewBag.Year = y;

            return View(report);
        }
    }
}
