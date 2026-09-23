using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StudentManagement.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Display name")]
        public string DisplayName { get; set; } = string.Empty;

        public string? CurrentProfilePicturePath { get; set; }

        [Display(Name = "Profile picture")]
        public IFormFile? ProfilePicture { get; set; }
    }
}
