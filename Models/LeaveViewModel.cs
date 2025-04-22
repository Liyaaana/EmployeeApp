using System;

namespace EmployeeApp.Models
{
    public class LeaveViewModel
    {
        public int Id { get; set; }
        public string ReadableEmployeeId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public string Department { get; set; }
        public int Days { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime AppliedDate { get; set; }

        public LeaveViewModel() { }

        public LeaveViewModel(int id, string employeeName, DateTime from, DateTime to)
        {
            Id = id;
            EmployeeName = employeeName;
            From = from;
            To = to;
        }
    }
}
