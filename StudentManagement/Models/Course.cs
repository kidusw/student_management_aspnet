using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int CreditHours { get; set; }

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Registration> Registrations { get; set; }
            = new List<Registration>();
    }
}