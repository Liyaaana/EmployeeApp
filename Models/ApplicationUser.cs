using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EmployeeApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; }
        public string Department { get; set; }

        public int? EmployeeId { get; set; }

        // Navigation property
        public Employee Employee { get; set; }
        public string EmployeeCode { get; set; }

        public decimal Salary { get; set; }

        public bool IsFirstLogin { get; set; } = true;
        public ICollection<EmployeeAuditLog> AuditLogs { get; set; }
    }
}
