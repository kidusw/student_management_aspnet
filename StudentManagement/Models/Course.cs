using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [Display(Name = "Department")]
        [ValidateNever]
        public Department Department { get; set; } = null!;

        // Navigation property
        public ICollection<Registration> Registrations { get; set; }
            = new List<Registration>();
    }
}