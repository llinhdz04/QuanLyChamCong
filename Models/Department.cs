using System.ComponentModel.DataAnnotations;

namespace QuanLyChamCong.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string DepartmentName { get; set; } = null!;


        public string? Description { get; set; }
    }
}
