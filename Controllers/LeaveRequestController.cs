using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;
using QuanLyChamCong.Models;

namespace QuanLyChamCong.Controllers
{
    public class LeaveRequestController : Controller
    {
        private readonly AppDbContext _context;
        private const int PAGE_SIZE = 10;

        public LeaveRequestController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(
            string? search,
            string? filterStatus = "all",
            string  sortBy       = "startdate",
            string  sortDir      = "desc",
            int     page         = 1)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var query = _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(l =>
                    (l.LeaveType != null && l.LeaveType.ToLower().Contains(s)) ||
                    (l.Reason    != null && l.Reason.ToLower().Contains(s))    ||
                    l.Status.ToLower().Contains(s));
            }

            if (filterStatus == "pending")  query = query.Where(l => l.Status == "Pending");
            if (filterStatus == "approved") query = query.Where(l => l.Status == "Approved");
            if (filterStatus == "rejected") query = query.Where(l => l.Status == "Rejected");

            query = (sortBy, sortDir) switch
            {
                ("startdate", "asc")  => query.OrderBy(l => l.StartDate),
                ("startdate", "desc") => query.OrderByDescending(l => l.StartDate),
                ("enddate",   "asc")  => query.OrderBy(l => l.EndDate),
                ("enddate",   "desc") => query.OrderByDescending(l => l.EndDate),
                ("leavetype", "asc")  => query.OrderBy(l => l.LeaveType),
                ("leavetype", "desc") => query.OrderByDescending(l => l.LeaveType),
                ("status",    "asc")  => query.OrderBy(l => l.Status),
                ("status",    "desc") => query.OrderByDescending(l => l.Status),
                _                    => query.OrderByDescending(l => l.StartDate)
            };

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

            var all = _context.LeaveRequests.Where(l => l.UserId == userId);
            ViewBag.CountAll      = all.Count();
            ViewBag.CountPending  = all.Count(l => l.Status == "Pending");
            ViewBag.CountApproved = all.Count(l => l.Status == "Approved");
            ViewBag.CountRejected = all.Count(l => l.Status == "Rejected");
            return View(items);
        }

        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LeaveRequest model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
                return View(model);
            }
            model.UserId = userId;
            model.Status = "Pending";
            _context.LeaveRequests.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Đã gửi đơn nghỉ phép, chờ Admin duyệt!";
            return RedirectToAction("Index");
        }

        public IActionResult All(
            string? search,
            string? filterStatus = "all",
            string  sortBy       = "startdate",
            string  sortDir      = "desc",
            int     page         = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var query = _context.LeaveRequests.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(l =>
                    l.UserId.ToLower().Contains(s) ||
                    (l.LeaveType != null && l.LeaveType.ToLower().Contains(s)) ||
                    (l.Reason    != null && l.Reason.ToLower().Contains(s)));
            }

            if (filterStatus == "pending")  query = query.Where(l => l.Status == "Pending");
            if (filterStatus == "approved") query = query.Where(l => l.Status == "Approved");
            if (filterStatus == "rejected") query = query.Where(l => l.Status == "Rejected");

            query = (sortBy, sortDir) switch
            {
                ("startdate", "asc")  => query.OrderBy(l => l.StartDate),
                ("startdate", "desc") => query.OrderByDescending(l => l.StartDate),
                ("userid",    "asc")  => query.OrderBy(l => l.UserId),
                ("userid",    "desc") => query.OrderByDescending(l => l.UserId),
                ("leavetype", "asc")  => query.OrderBy(l => l.LeaveType),
                ("leavetype", "desc") => query.OrderByDescending(l => l.LeaveType),
                ("status",    "asc")  => query.OrderBy(l => l.Status),
                ("status",    "desc") => query.OrderByDescending(l => l.Status),
                _                    => query.OrderByDescending(l => l.StartDate)
            };

            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = query.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            ViewBag.Search        = search;
            ViewBag.FilterStatus  = filterStatus;
            ViewBag.SortBy        = sortBy;
            ViewBag.SortDir       = sortDir;
            ViewBag.Page          = page;
            ViewBag.TotalPages    = totalPages;
            ViewBag.Total         = total;
            ViewBag.CountAll      = _context.LeaveRequests.Count();
            ViewBag.CountPending  = _context.LeaveRequests.Count(l => l.Status == "Pending");
            ViewBag.CountApproved = _context.LeaveRequests.Count(l => l.Status == "Approved");
            ViewBag.CountRejected = _context.LeaveRequests.Count(l => l.Status == "Rejected");
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();
            var r = _context.LeaveRequests.Find(id);
            if (r == null) return NotFound();
            r.Status       = "Approved";
            r.ApprovedBy   = HttpContext.Session.GetString("UserName");
            r.ApprovedDate = DateTime.Now;
            _context.SaveChanges();
            TempData["Success"] = "Đã duyệt đơn";
            return RedirectToAction("All");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();
            var r = _context.LeaveRequests.Find(id);
            if (r == null) return NotFound();
            r.Status       = "Rejected";
            r.ApprovedBy   = HttpContext.Session.GetString("UserName");
            r.ApprovedDate = DateTime.Now;
            _context.SaveChanges();
            TempData["Success"] = "Đã từ chối đơn";
            return RedirectToAction("All");
        }
    }
}
