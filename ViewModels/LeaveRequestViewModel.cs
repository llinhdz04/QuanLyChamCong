using System.ComponentModel.DataAnnotations;

namespace QuanLyChamCong.ViewModels
{
    public class LeaveRequestViewModel
    {
        public int LeaveRequestId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại nghỉ phép")]
        public string LeaveType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public string? UserId { get; set; }
    }
}
