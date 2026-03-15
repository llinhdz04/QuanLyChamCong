namespace QuanLyChamCong.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TodayHours { get; set; }
        public bool HasCheckedIn { get; set; }
        public bool HasCheckedOut { get; set; }
        public decimal MonthlyHours { get; set; }
        public int WorkDaysThisMonth { get; set; }
        public int PendingLeaves { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}