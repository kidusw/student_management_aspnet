using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentManagement.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string? Grade { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        // Navigation properties
        // ValidateNever: these aren't posted by the Create/Edit forms (only the
        // Ids are), so without this the model binder treats these non-nullable
        // reference types as implicitly required and ModelState.IsValid is
        // always false, silently re-rendering the form with no visible error.
        [ForeignKey(nameof(StudentId))]
        [ValidateNever]
        public Student Student { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        [ValidateNever]
        public Course Course { get; set; } = null!;
    }
}