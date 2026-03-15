namespace QuanLyChamCong.ViewModels
{
    public class AttendanceViewModel
    {
        public string EmployeeName { get; set; }
        public DateTime TodayDate { get; set; }

        public bool HasCheckedIn { get; set; }
        public bool HasCheckedOut { get; set; }

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double TotalHours { get; set; }
        public decimal TotalWorkingHours { get; set; }
    }
}
