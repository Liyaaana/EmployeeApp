using System.ComponentModel.DataAnnotations;

namespace EmployeeApp.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }

        [Required]
        public decimal Salary { get; set; }
        public string EmployeeCode { get; set; }
        public ApplicationUser ApplicationUser { get; set; }


    }
}
