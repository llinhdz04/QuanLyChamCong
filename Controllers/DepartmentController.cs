using Microsoft.AspNetCore.Mvc;
using QuanLyChamCong.Data;
using QuanLyChamCong.Models;

namespace QuanLyChamCong.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? search, string sortBy = "name", string sortDir = "asc", int page = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            const int PAGE_SIZE = 10;
            var query = _context.Departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(d =>
                    d.DepartmentName.ToLower().Contains(s) ||
                    (d.Description != null && d.Description.ToLower().Contains(s)));
            }

            query = (sortBy, sortDir) switch
            {
                ("name",  "asc")  => query.OrderBy(d => d.DepartmentName),
                ("name",  "desc") => query.OrderByDescending(d => d.DepartmentName),
                ("id",    "asc")  => query.OrderBy(d => d.DepartmentId),
                ("id",    "desc") => query.OrderByDescending(d => d.DepartmentId),
                _ => query.OrderBy(d => d.DepartmentName)
            };

            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            var items      = query.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToList();

            var empCount = _context.ApplicationUsers
                .Where(u => u.DepartmentId != null)
                .GroupBy(u => u.DepartmentId)
                .ToDictionary(g => g.Key!.Value, g => g.Count());

            ViewBag.Search     = search;
            ViewBag.SortBy     = sortBy;
            ViewBag.SortDir    = sortDir;
            ViewBag.Page       = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total      = total;
            ViewBag.EmpCount   = empCount;
            return View(items);
        }

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Department dept)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            if (_context.Departments.Any(d => d.DepartmentName == dept.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", "Tên phòng ban đã tồn tại!");
                return View(dept);
            }
            _context.Departments.Add(dept);
            _context.SaveChanges();
            TempData["Success"] = $"Đã thêm phòng ban \"{dept.DepartmentName}\"";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Department model)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            if (_context.Departments.Any(d => d.DepartmentName == model.DepartmentName && d.DepartmentId != id))
            {
                ModelState.AddModelError("DepartmentName", "Tên phòng ban đã tồn tại!");
                return View(model);
            }
            dept.DepartmentName = model.DepartmentName;
            dept.Description    = model.Description;
            _context.SaveChanges();
            TempData["Success"] = "Đã cập nhật phòng ban!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            var users = _context.ApplicationUsers.Where(u => u.DepartmentId == id).ToList();
            foreach (var u in users) u.DepartmentId = null;

            _context.Departments.Remove(dept);
            _context.SaveChanges();
            TempData["Success"] = $"Đã xóa phòng ban \"{dept.DepartmentName}\"";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToAction("Index", "Home");

            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            var employees = _context.ApplicationUsers
                .Where(u => u.DepartmentId == id)
                .ToList();

            ViewBag.Employees = employees;
            return View(dept);
        }
    }
}
