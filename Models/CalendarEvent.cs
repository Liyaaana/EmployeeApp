namespace EmployeeApp.Models
{
    public class CalendarEvent
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // "Leave" or "Holiday"
        public string EmployeeName { get; set; } // for admin view
    }
}
