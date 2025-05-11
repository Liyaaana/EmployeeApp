using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Models;

namespace EmployeeApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<EmployeeDto> EmployeeDtos { get; set; } // result from stored procedure

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Set decimal precision for Employee.Salary
            builder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            // One-to-one relationship between ApplicationUser and Employee
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.ApplicationUser)
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId);

            // Configure EmployeeDto for stored procedure 
            builder.Entity<EmployeeDto>().HasNoKey().ToView(null);
        }
    }
}
