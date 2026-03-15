using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;
using QuanLyChamCong.ViewModels;

namespace QuanLyChamCong.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = _context.ApplicationUsers.Find(userId);
            if (user == null) return NotFound();

            var dept = user.DepartmentId.HasValue
                ? _context.Departments.Find(user.DepartmentId.Value)
                : null;

            ViewBag.DepartmentName = dept?.DepartmentName ?? "Chưa phân công";
            ViewBag.Departments    = _context.Departments.ToList();

            var today = DateTime.Today;
            ViewBag.TotalDays    = _context.Attendances.Count(a => a.UserId == userId && a.CheckInTime != null);
            ViewBag.TotalHours   = _context.Attendances.Where(a => a.UserId == userId).Sum(a => a.TotalHours) ?? 0m;
            ViewBag.PendingLeave = _context.LeaveRequests.Count(l => l.UserId == userId && l.Status == "Pending");
            ViewBag.ThisMonth    = _context.Attendances
                .Count(a => a.UserId == userId && a.CheckInTime != null
                         && a.Date.Month == today.Month && a.Date.Year == today.Year);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(string fullName, string email,
                                    string? phoneNumber, string? position,
                                    int? departmentId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = _context.ApplicationUsers.Find(userId);
            if (user == null) return NotFound();

            user.FullName     = fullName;
            user.Email        = email;
            user.PhoneNumber  = phoneNumber;
            user.Position     = position;
            user.DepartmentId = departmentId;
            _context.SaveChanges();

            HttpContext.Session.SetString("FullName", fullName ?? user.UserName);
            TempData["Success"] = "Đã cập nhật thông tin cá nhân!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword,
                                            string newPassword,
                                            string confirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = _context.ApplicationUsers.Find(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            {
                TempData["PwdError"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction("Index");
            }
            if (newPassword != confirmPassword)
            {
                TempData["PwdError"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("Index");
            }
            if (newPassword.Length < 6)
            {
                TempData["PwdError"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return RedirectToAction("Index");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.SaveChanges();
            TempData["PwdSuccess"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index");
        }
    }
}
