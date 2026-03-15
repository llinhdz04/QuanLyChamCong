using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;
using QuanLyChamCong.ViewModels;

namespace QuanLyChamCong.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            // Chấm công hôm nay
            var todayRecord = _context.Attendances
                .FirstOrDefault(a => a.UserId == userId && a.Date == today);

            // Tổng giờ tháng này
            var monthlyHours = _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= thisMonth)
                .Sum(a => a.TotalHours) ?? 0m;

            // Số ngày đi làm tháng này
            var workDays = _context.Attendances
                .Count(a => a.UserId == userId && a.Date >= thisMonth && a.CheckInTime != null);

            // Đơn nghỉ phép đang chờ duyệt
            var pendingLeaves = _context.LeaveRequests
                .Count(l => l.UserId == userId && l.Status == "Pending");

            var vm = new DashboardViewModel
            {
                TodayHours    = todayRecord?.TotalHours ?? 0m,
                HasCheckedIn  = todayRecord?.CheckInTime != null,
                HasCheckedOut = todayRecord?.CheckOutTime != null,
                MonthlyHours  = monthlyHours,
                WorkDaysThisMonth = workDays,
                PendingLeaves = pendingLeaves,
                CheckInTime   = todayRecord?.CheckInTime,
                CheckOutTime  = todayRecord?.CheckOutTime,
            };

            return View(vm);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
