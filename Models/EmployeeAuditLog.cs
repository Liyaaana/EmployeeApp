namespace EmployeeApp.Models
{
    public class EmployeeAuditLog
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string ActionType { get; set; }
        public DateTime ActionTime { get; set; }      
        public string PageVisited { get; set; }

        public ApplicationUser Employee { get; set; } //  navigation property
    }
}
