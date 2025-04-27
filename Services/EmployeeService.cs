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

            // Generate a random number between 1000 and 9999
            Random rand = new Random();
            int randomNumber = rand.Next(1000, 9999);  // Generates a number between 1000 and 9999 (inclusive of 1000, exclusive of 9999)

            // Combine namePart with random number
            string employeeCode = $"{namePart}{randomNumber}";

            return employeeCode;
        }
    }
}
