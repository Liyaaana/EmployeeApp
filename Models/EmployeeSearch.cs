namespace EmployeeApp.Models
{
    public class EmployeeSearch
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
    }
}
