namespace StudentManagement.Models
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }

        public int TotalCourses { get; set; }

        public int TotalDepartments { get; set; }

        public int TotalRegistrations { get; set; }

        public int ActiveRegistrations { get; set; }

        public int CompletedRegistrations { get; set; }

        public int DroppedRegistrations { get; set; }

        public int CancelledRegistrations { get; set; }

        public List<DepartmentSummary> DepartmentSummaries { get; set; } = new();

        public List<Registration> RecentRegistrations { get; set; } = new();
    }

    public class DepartmentSummary
    {
        public string DepartmentName { get; set; } = string.Empty;

        public int StudentCount { get; set; }

        public int CourseCount { get; set; }
    }
}
