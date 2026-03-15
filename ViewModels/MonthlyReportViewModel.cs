namespace QuanLyChamCong.ViewModels
{

    public class MonthlyReportViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalHours { get; set; }
        public decimal Salary { get; set; }
        public int TotalWorkDays { get; set; }
        public int TotalAbsentDays { get; set; }
    }
}
