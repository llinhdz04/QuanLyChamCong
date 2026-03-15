using QuanLyChamCong.Data;
using QuanLyChamCong.Models;
using QuanLyChamCong.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChamCong.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private const int PAGE_SIZE = 10;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var today = DateTime.Today;
            var vm = new AdminDashboardViewModel
            {
                TotalEmployees   = _context.ApplicationUsers.Count(u => u.Role == "User"),
                CheckedInToday   = _context.Attendances.Count(a => a.Date == today),
                PendingLeaves    = _context.LeaveRequests.Count(l => l.Status == "Pending"),
                TotalHoursMonth  = _context.Attendances
                    .Where(a => a.Date.Month == today.Month && a.Date.Year == today.Year)
                    .Sum(a => a.TotalHours) ?? 0m,
                RecentAttendances = _context.Attendances
                    .Where(a => a.Date == today)
                    .OrderByDescending(a => a.CheckInTime)
                    .Take(10).ToList(),
                RecentLeaves = _context.LeaveRequests
                    .Where(l => l.Status == "Pending")
                    .OrderByDescending(l => l.StartDate)
                    .Take(5).ToList()
            };
            return View(vm);
        }

        // USERS
        public IActionResult Users(
            string? search,
            string  sortBy   = "fullname",
            string  sortDir  = "asc",
            int     page     = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            //search
            var query = _context.ApplicationUsers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName!.ToLower().Contains(s) ||
                    u.UserName.ToLower().Contains(s)  ||
                    u.Email.ToLower().Contains(s)     ||
                    (u.Position != null && u.Position.ToLower().Contains(s)));
            }

            //sort
            query = (sortBy, sortDir) switch
            {
                ("fullname",  "asc")  => query.OrderBy(u => u.FullName),
                ("fullname",  "desc") => query.OrderByDescending(u => u.FullName),
                ("username",  "asc")  => query.OrderBy(u => u.UserName),
                ("username",  "desc") => query.OrderByDescending(u => u.UserName),
                ("email",     "asc")  => query.OrderBy(u => u.Email),
                ("email",     "desc") => query.OrderByDescending(u => u.Email),
                ("hiredate",  "asc")  => query.OrderBy(u => u.HireDate),
                ("hiredate",  "desc") => query.OrderByDescending(u => u.HireDate),
                ("role",      "asc")  => query.OrderBy(u => u.Role),
                ("role",      "desc") => query.OrderByDescending(u => u.Role),
                _                    => query.OrderBy(u => u.FullName)
            };

            //paging
            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = query.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            ViewBag.Search     = search;
            ViewBag.SortBy     = sortBy;
            ViewBag.SortDir    = sortDir;
            ViewBag.Page       = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total      = total;
            return View(items);
        }

        // CREATE / EDIT / DELETE USER
        public IActionResult CreateUser()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(ApplicationUser user)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            if (_context.ApplicationUsers.Any(u => u.UserName == user.UserName))
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                return View(user);
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            if (string.IsNullOrEmpty(user.Role)) user.Role = "User";
            user.Id = Guid.NewGuid().ToString();
            _context.ApplicationUsers.Add(user);
            _context.SaveChanges();
            TempData["Success"] = $"Đã thêm nhân viên {user.FullName ?? user.UserName}";
            return RedirectToAction("Users");
        }

        public IActionResult EditUser(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");
            var user = _context.ApplicationUsers.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(string id, ApplicationUser model, string? newPassword)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var user = _context.ApplicationUsers.Find(id);
            if (user == null) return NotFound();

            user.FullName    = model.FullName;
            user.Email       = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Position    = model.Position;
            user.Role        = model.Role;

            if (!string.IsNullOrWhiteSpace(newPassword))
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.SaveChanges();
            TempData["Success"] = "Đã cập nhật thông tin";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var user = _context.ApplicationUsers.Find(id);
            if (user != null)
            {
                _context.ApplicationUsers.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = $"Đã xóa nhân viên {user.FullName ?? user.UserName}";
            }
            return RedirectToAction("Users");
        }

        // USER HOURS
        public IActionResult UserHours(
            string  id,
            int?    month,
            int?    year,
            string? sortBy  = "date",
            string  sortDir = "desc",
            int     page    = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var user = _context.ApplicationUsers.Find(id);
            if (user == null) return NotFound();

            int m = month ?? DateTime.Today.Month;
            int y = year  ?? DateTime.Today.Year;

            //quẻry
            var query = _context.Attendances
                .Where(a => a.UserId == id && a.Date.Month == m && a.Date.Year == y)
                .AsQueryable();

            //sort
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

            //paging
            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = query.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            ViewBag.User       = user;
            ViewBag.Month      = m;
            ViewBag.Year       = y;
            ViewBag.TotalHours = _context.Attendances
                .Where(a => a.UserId == id && a.Date.Month == m && a.Date.Year == y)
                .Sum(a => a.TotalHours) ?? 0m;
            ViewBag.WorkDays   = _context.Attendances
                .Count(a => a.UserId == id && a.Date.Month == m && a.Date.Year == y && a.CheckInTime != null);
            ViewBag.SortBy     = sortBy;
            ViewBag.SortDir    = sortDir;
            ViewBag.Page       = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total      = total;
            return View(items);
        }

        // SCHEDULES
        public IActionResult Schedules(
            string? search,
            string? filterStatus = "all",
            string  sortBy       = "workdate",
            string  sortDir      = "desc",
            int     page         = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var query = _context.WorkSchedules.AsQueryable();

            //search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    x.UserId.ToLower().Contains(s) ||
                    (x.Shift != null && x.Shift.ToLower().Contains(s)));
            }

            if (filterStatus == "pending")  query = query.Where(x => !x.IsApproved);
            if (filterStatus == "approved") query = query.Where(x => x.IsApproved);

            //sort
            query = (sortBy, sortDir) switch
            {
                ("workdate", "asc")  => query.OrderBy(x => x.WorkDate),
                ("workdate", "desc") => query.OrderByDescending(x => x.WorkDate),
                ("userid",   "asc")  => query.OrderBy(x => x.UserId),
                ("userid",   "desc") => query.OrderByDescending(x => x.UserId),
                ("shift",    "asc")  => query.OrderBy(x => x.Shift),
                ("shift",    "desc") => query.OrderByDescending(x => x.Shift),
                _                   => query.OrderByDescending(x => x.WorkDate)
            };

            //paging
            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = query.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            ViewBag.Search       = search;
            ViewBag.FilterStatus = filterStatus;
            ViewBag.SortBy       = sortBy;
            ViewBag.SortDir      = sortDir;
            ViewBag.Page         = page;
            ViewBag.TotalPages   = totalPages;
            ViewBag.Total        = total;
            ViewBag.PendingCount  = _context.WorkSchedules.Count(x => !x.IsApproved);
            ViewBag.ApprovedCount = _context.WorkSchedules.Count(x => x.IsApproved);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveSchedule(int id)
        {
            var s = _context.WorkSchedules.Find(id);
            if (s != null) { s.IsApproved = true; _context.SaveChanges(); }
            TempData["Success"] = "Đã duyệt lịch làm việc";
            return RedirectToAction("Schedules");
        }

        // REPORT
        public IActionResult Report(
            string? search,
            int?    month,
            int?    year,
            string  sortBy  = "totalhours",
            string  sortDir = "desc",
            int     page    = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            int m = month ?? DateTime.Today.Month;
            int y = year  ?? DateTime.Today.Year;

            //  report từ db
            var reportQuery = _context.Attendances
                .Where(a => a.Date.Month == m && a.Date.Year == y)
                .GroupBy(a => a.UserId)
                .Select(g => new ReportVM
                {
                    UserId     = g.Key,
                    FullName   = _context.ApplicationUsers
                                    .Where(u => u.Id == g.Key)
                                    .Select(u => u.FullName ?? u.UserName)
                                    .FirstOrDefault() ?? g.Key,
                    TotalHours = g.Sum(a => a.TotalHours) ?? 0m,
                    Salary     = (g.Sum(a => a.TotalHours) ?? 0m) * 30000m
                })
                .AsEnumerable();

            //search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                reportQuery = reportQuery.Where(r =>
                    r.FullName.ToLower().Contains(s) ||
                    r.UserId.ToLower().Contains(s));
            }

            //sort
            reportQuery = (sortBy, sortDir) switch
            {
                ("fullname",   "asc")  => reportQuery.OrderBy(r => r.FullName),
                ("fullname",   "desc") => reportQuery.OrderByDescending(r => r.FullName),
                ("totalhours", "asc")  => reportQuery.OrderBy(r => r.TotalHours),
                ("totalhours", "desc") => reportQuery.OrderByDescending(r => r.TotalHours),
                ("salary",     "asc")  => reportQuery.OrderBy(r => r.Salary),
                ("salary",     "desc") => reportQuery.OrderByDescending(r => r.Salary),
                _                     => reportQuery.OrderByDescending(r => r.TotalHours)
            };

            var reportList = reportQuery.ToList();

            //paging
            int total      = reportList.Count;
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = reportList.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            ViewBag.Search     = search;
            ViewBag.Month      = m;
            ViewBag.Year       = y;
            ViewBag.SortBy     = sortBy;
            ViewBag.SortDir    = sortDir;
            ViewBag.Page       = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total      = total;
            ViewBag.GrandTotal = reportList.Sum(r => r.Salary);
            ViewBag.TotalHours = reportList.Sum(r => r.TotalHours);
            return View(items);
        }
    }
}
