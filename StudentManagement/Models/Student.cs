using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public int Age { get; set; }

        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string MobileNumber { get; set; } = string.Empty;

        public ICollection<Registration> Registrations { get; set; }
       = new List<Registration>();
    }
}
