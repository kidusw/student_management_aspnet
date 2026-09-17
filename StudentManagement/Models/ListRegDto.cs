using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentManagement.Models
{
    public class ListRegDto
    {
        public String? searchString { get; set; } = string.Empty;

        public List<RegisterationDto> registrations { get; set; }

        public int? SearchCourseId { get; set; }


        public List<SelectListItem> CourseList { get; set; }

    }
}
