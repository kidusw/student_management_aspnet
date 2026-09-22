using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;
using System.Diagnostics;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var departmentSummaries = await _context.Departments
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentSummary
                {
                    DepartmentName = d.Name,
                    StudentCount = d.Students.Count,
                    CourseCount = d.Courses.Count,
                    RegistrationCount = d.Students.Sum(s => s.Registrations.Count)
                })
                .ToListAsync();

            var registrationCounts = await _context.Registrations
                .GroupBy(r => r.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            int CountFor(RegistrationStatus status) =>
                registrationCounts.FirstOrDefault(c => c.Key == status)?.Count ?? 0;

            var vm = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalRegistrations = registrationCounts.Sum(c => c.Count),
                ActiveRegistrations = CountFor(RegistrationStatus.Active),
                CompletedRegistrations = CountFor(RegistrationStatus.Completed),
                DroppedRegistrations = CountFor(RegistrationStatus.Dropped),
                CancelledRegistrations = CountFor(RegistrationStatus.Cancelled),
                DepartmentSummaries = departmentSummaries,
                RecentRegistrations = await _context.Registrations
                    .Include(r => r.Student)
                    .Include(r => r.Course)
                    .OrderByDescending(r => r.RegistrationDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
