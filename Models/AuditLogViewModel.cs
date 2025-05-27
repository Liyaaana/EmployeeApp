namespace EmployeeApp.Models
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string PageVisited { get; set; }
        public string ActionType { get; set; }
        public DateTime ActionTime { get; set; }

        public int TotalCount { get; set; }
    }
}
