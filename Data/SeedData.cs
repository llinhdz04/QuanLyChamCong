using QuanLyChamCong.Models;

namespace QuanLyChamCong.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.ApplicationUsers.Any())
                return;

            var admin = new ApplicationUser
            {
                Id = "admin",
                UserName = "admin",
                FullName = "Administrator",
                Email = "admin@company.com",
                PhoneNumber = "0123456789",
                Position = "Admin",
                Role = "Admin",
                HireDate = DateTime.Now,

                Password = BCrypt.Net.BCrypt.HashPassword("123456")
            };

            context.ApplicationUsers.Add(admin);
            context.SaveChanges();
        }
    }
}
