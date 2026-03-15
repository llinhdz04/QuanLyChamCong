using System.ComponentModel.DataAnnotations;

namespace QuanLyChamCong.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public string? LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }
    }
}
