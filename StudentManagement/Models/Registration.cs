using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StudentManagement.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Display(Name ="Student Name")]
        public int StudentId { get; set; }

        [Display(Name = "Course Name")]
        public int CourseId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;


        [StringLength(20)]
        public string? Grade { get; set; }

        public RegistrationStatus Status { get; set; } = RegistrationStatus.Active;

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