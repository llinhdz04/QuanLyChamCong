using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;

namespace QuanLyChamCong.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var leaves = _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.ApprovedDate ?? l.StartDate)
                .Take(30)
                .ToList();

            var schedules = _context.WorkSchedules
                .Where(s => s.UserId == userId && s.IsApproved)
                .OrderByDescending(s => s.WorkDate)
                .Take(20)
                .ToList();

            var unreadLeave    = leaves.Count(l => l.Status != "Pending" && l.ApprovedDate >= DateTime.Today.AddDays(-7));
            var unreadSchedule = schedules.Count(s => s.WorkDate >= DateTime.Today.AddDays(-7));

            ViewBag.Leaves          = leaves;
            ViewBag.Schedules       = schedules;
            ViewBag.UnreadLeave     = unreadLeave;
            ViewBag.UnreadSchedule  = unreadSchedule;
            ViewBag.TotalUnread     = unreadLeave + unreadSchedule;
            return View();
        }

        public IActionResult Count()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Json(new { count = 0 });

            var count = _context.LeaveRequests
                .Count(l => l.UserId == userId
                    && l.Status != "Pending"
                    && l.ApprovedDate >= DateTime.Today.AddDays(-7));

            count += _context.WorkSchedules
                .Count(s => s.UserId == userId
                    && s.IsApproved
                    && s.WorkDate >= DateTime.Today.AddDays(-7));

            return Json(new { count });
        }
    }
}
