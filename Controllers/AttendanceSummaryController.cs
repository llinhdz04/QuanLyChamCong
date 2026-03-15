using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;

namespace QuanLyChamCong.Controllers
{
    public class AttendanceSummaryController : Controller
    {
        private readonly AppDbContext _context;

        public AttendanceSummaryController(AppDbContext context)
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

            int daysInMonth = DateTime.DaysInMonth(y, m);

            int workDays     = records.Count(a => a.CheckInTime != null);
            int completeDays = records.Count(a => a.CheckInTime != null && a.CheckOutTime != null);
            int lateDays     = records.Count(a => a.CheckInTime.HasValue && a.CheckInTime.Value.Hour >= 9);
            decimal totalHours = records.Sum(a => a.TotalHours) ?? 0m;
            decimal avgHours   = workDays > 0 ? Math.Round(totalHours / workDays, 1) : 0m;

            var calendarDays = Enumerable.Range(1, daysInMonth).Select(day =>
            {
                var date = new DateTime(y, m, day);
                var rec  = records.FirstOrDefault(a => a.Date.Date == date);
                string status = "absent";
                if (rec?.CheckInTime != null && rec.CheckOutTime != null) status = "complete";
                else if (rec?.CheckInTime != null) status = "working";
                return new { Day = day, Date = date, Record = rec, Status = status };
            }).ToList();

            var trend = Enumerable.Range(5, -5).Concat(new[] { 0 })
                .Select(i => {
                    var d  = new DateTime(y, m, 1).AddMonths(-i);
                    var h  = _context.Attendances
                        .Where(a => a.UserId == userId && a.Date.Month == d.Month && a.Date.Year == d.Year)
                        .Sum(a => a.TotalHours) ?? 0m;
                    var wd = _context.Attendances
                        .Count(a => a.UserId == userId && a.Date.Month == d.Month && a.Date.Year == d.Year && a.CheckInTime != null);
                    return new { Label = $"T{d.Month}", Hours = h, Days = wd };
                }).ToList();

            var leaves = _context.LeaveRequests
                .Where(l => l.UserId == userId
                    && ((l.StartDate.Month == m && l.StartDate.Year == y)
                     || (l.EndDate.Month == m && l.EndDate.Year == y)))
                .ToList();

            ViewBag.Month        = m;
            ViewBag.Year         = y;
            ViewBag.DaysInMonth  = daysInMonth;
            ViewBag.WorkDays     = workDays;
            ViewBag.CompleteDays = completeDays;
            ViewBag.LateDays     = lateDays;
            ViewBag.TotalHours   = totalHours;
            ViewBag.AvgHours     = avgHours;
            ViewBag.Salary       = totalHours * 30000m;
            ViewBag.CalendarDays = calendarDays;
            ViewBag.Trend        = trend;
            ViewBag.Leaves       = leaves;

            return View();
        }
    }
}
