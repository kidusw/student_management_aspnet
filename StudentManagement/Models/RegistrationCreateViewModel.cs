using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentManagement.Models
{
    public class RegistrationCreateViewModel
    {
        public Registration Registration { get; set; } = new Registration();

        public List<SelectListItem> Students { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Courses { get; set; } = new List<SelectListItem>();
    }
}
