using System.ComponentModel.DataAnnotations;

namespace QuanLyChamCong.Models
{
    public class ApplicationUser
    {
        [Key]
        public string Id { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!; 

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }

        public int? DepartmentId { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;

        public string Role { get; set; } = "User";
    }
}
