using System.ComponentModel.DataAnnotations;

namespace QuanLyChamCong.Models
{
    public class WorkSchedule
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public DateTime WorkDate { get; set; }

        public string? Shift { get; set; }

        public bool IsApproved { get; set; }
    }
}
