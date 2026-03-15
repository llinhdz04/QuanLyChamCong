using System.ComponentModel.DataAnnotations;

namespace QuanLyChamCong.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public DateTime Date { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public decimal? TotalHours { get; set; }

        public string? Status { get; set; }

        public string? Note { get; set; }
    }
}
