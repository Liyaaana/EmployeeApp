using System.Linq;
using EmployeeApp.Data;
using EmployeeApp.Models;

namespace EmployeeApp.Services
{
    public class EmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GenerateEmployeeCode(string fullName)
        {
            // Ensure that the full name is not null or empty
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException("Full name is required.");
            }

            // Extract the first three letters of the name, padding with the first letter if necessary
            string namePart = new string(fullName.Substring(0, Math.Min(3, fullName.Length)).ToUpper().ToCharArray());
            while (namePart.Length < 3)
            {
                namePart += namePart[0];
            }

            // Generate the numeric part (e.g., 0001, 0002)
            var lastEmployee = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.EmployeeCode))  // Only consider users with an EmployeeCode
                .OrderByDescending(u => u.EmployeeCode)
                .FirstOrDefault();

            var lastNumber = lastEmployee == null ? 0 : int.Parse(lastEmployee.EmployeeCode.Substring(3));
            var newNumber = lastNumber + 1;

            // Generate the employee code: e.g., AVA027
            string employeeCode = $"{namePart}{newNumber:D4}";  // Ensure the number is 4 digits

            return employeeCode;
        }

    }
}
