using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;
using QuanLyChamCong.Models;

namespace QuanLyChamCong.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly AppDbContext _context;

        public ScheduleController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedules = _context.WorkSchedules
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.WorkDate)
                .ToList();

            return View(schedules);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(WorkSchedule s)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (_context.WorkSchedules.Any(x => x.UserId == userId && x.WorkDate == s.WorkDate))
            {
                TempData["Error"] = "Bạn đã đăng ký lịch cho ngày này rồi!";
                return RedirectToAction("Index");
            }

            s.UserId = userId;
            s.IsApproved = false;

            _context.WorkSchedules.Add(s);
            _context.SaveChanges();

            TempData["Success"] = "Đã gửi đăng ký lịch làm, chờ Admin duyệt!";
            return RedirectToAction("Index");
        }
    }
}