using QuanLyChamCong.Models;

namespace QuanLyChamCong.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int CheckedInToday { get; set; }
        public int PendingLeaves { get; set; }
        public decimal TotalHoursMonth { get; set; }
        public List<Attendance> RecentAttendances { get; set; } = new();
        public List<LeaveRequest> RecentLeaves { get; set; } = new();
    }
}