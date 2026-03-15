using Microsoft.EntityFrameworkCore;
using QuanLyChamCong.Models;

namespace QuanLyChamCong.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser: UserName unique
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // ApplicationUser: Email unique
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Attendance: mỗi user chỉ có 1 bản ghi mỗi ngày
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.UserId, a.Date })
                .IsUnique();

            // WorkSchedule: mỗi user chỉ có 1 ca mỗi ngày
            modelBuilder.Entity<WorkSchedule>()
                .HasIndex(s => new { s.UserId, s.WorkDate })
                .IsUnique();

            // FIX: ApplicationUser.Id là string → cấu hình tường minh
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Id);
                

            // Decimal precision cho TotalHours và Salary
            modelBuilder.Entity<Attendance>()
                .Property(a => a.TotalHours)
                .HasPrecision(8, 2);
        }
    }
}
