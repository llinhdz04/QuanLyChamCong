using QuanLyChamCong.Data;
using QuanLyChamCong.Models;
using QuanLyChamCong.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyChamCong.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;
        private const int PAGE_SIZE = 15;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var today      = DateTime.Today;
            var attendance = _context.Attendances.FirstOrDefault(x => x.UserId == userId && x.Date == today);

            var vm = new AttendanceViewModel
            {
                EmployeeName      = HttpContext.Session.GetString("FullName") ?? HttpContext.Session.GetString("UserName") ?? "—",
                TodayDate         = today,
                HasCheckedIn      = attendance?.CheckInTime  != null,
                HasCheckedOut     = attendance?.CheckOutTime != null,
                CheckInTime       = attendance?.CheckInTime,
                CheckOutTime      = attendance?.CheckOutTime,
                TotalWorkingHours = attendance?.TotalHours ?? 0m,
                TotalHours        = (double)(attendance?.TotalHours ?? 0m)
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CheckIn()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var today = DateTime.Today;
            if (_context.Attendances.Any(x => x.UserId == userId && x.Date == today))
                return Json(new { success = false, message = "Đã check-in hôm nay rồi" });

            _context.Attendances.Add(new Attendance { UserId = userId, Date = today, CheckInTime = DateTime.Now });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult CheckOut()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var record = _context.Attendances.FirstOrDefault(x => x.UserId == userId && x.Date == DateTime.Today);
            if (record == null)        return Json(new { success = false, message = "Chưa check-in" });
            if (record.CheckInTime == null)  return Json(new { success = false, message = "Dữ liệu check-in lỗi" });
            if (record.CheckOutTime != null) return Json(new { success = false, message = "Đã check-out rồi" });

            record.CheckOutTime = DateTime.Now;
            record.TotalHours   = (decimal)(record.CheckOutTime.Value - record.CheckInTime.Value).TotalHours;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // HISTORY
        public IActionResult History(
            string?   search,
            DateTime? fromDate,
            DateTime? toDate,
            string?   filterStatus = "all",
            string    sortBy       = "date",
            string    sortDir      = "desc",
            int       page         = 1)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var query = _context.Attendances
                .Where(a => a.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            if (filterStatus == "done")    query = query.Where(a => a.CheckOutTime != null);
            if (filterStatus == "working") query = query.Where(a => a.CheckInTime != null && a.CheckOutTime == null);

            query = (sortBy, sortDir) switch
            {
                ("date",       "asc")  => query.OrderBy(a => a.Date),
                ("date",       "desc") => query.OrderByDescending(a => a.Date),
                ("checkin",    "asc")  => query.OrderBy(a => a.CheckInTime),
                ("checkin",    "desc") => query.OrderByDescending(a => a.CheckInTime),
                ("totalhours", "asc")  => query.OrderBy(a => a.TotalHours),
                ("totalhours", "desc") => query.OrderByDescending(a => a.TotalHours),
                _                     => query.OrderByDescending(a => a.Date)
            };

            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = query.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            var allRecords   = _context.Attendances.Where(a => a.UserId == userId);
            var thisMonth    = allRecords.Where(a => a.Date.Month == DateTime.Today.Month && a.Date.Year == DateTime.Today.Year);

            ViewBag.Search       = search;
            ViewBag.FromDate     = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate       = toDate?.ToString("yyyy-MM-dd");
            ViewBag.FilterStatus = filterStatus;
            ViewBag.SortBy       = sortBy;
            ViewBag.SortDir      = sortDir;
            ViewBag.Page         = page;
            ViewBag.TotalPages   = totalPages;
            ViewBag.Total        = total;
            ViewBag.TotalHours   = allRecords.Sum(a => a.TotalHours) ?? 0m;
            ViewBag.WorkDays     = allRecords.Count(a => a.CheckInTime != null);
            ViewBag.ThisMonth    = thisMonth.Count(a => a.CheckInTime != null);
            return View(items);
        }
    }
}
