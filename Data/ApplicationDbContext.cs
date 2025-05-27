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
        public DbSet<EmployeeDto> EmployeeDtos { get; set; } // stores result from stored procedure
        
        public DbSet<EmployeeAuditLog> EmployeeAuditLogs { get; set; }
        public DbSet<AuditLogViewModel>AuditLogViewModel { get; set; }  
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

            /* Configure EmployeeDto for dashboard filter stored procedure
             * ToView(null) used to avoid mapping to any table or view
             */
            builder.Entity<EmployeeDto>().HasNoKey().ToView(null);

            // configure one-to-many relationship between EmployeeAuditLog and ApplicationUser entries
            builder.Entity<EmployeeAuditLog>()
                  .HasOne(e => e.Employee)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.EmployeeCode)
                  .HasPrincipalKey(u => u.EmployeeCode);

            //  configure AuditLogViewModel class as a read-only model not a real table to store result from stored procedure
            builder.Entity<AuditLogViewModel>().HasNoKey();

        }
    }
}
