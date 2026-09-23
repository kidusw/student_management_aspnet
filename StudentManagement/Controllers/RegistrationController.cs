
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Data;

[Route("Registration")]
[Authorize(Roles = "Admin,Staff,Viewer")]
public class RegistrationController : Controller
{
    private readonly ApplicationDbContext _context;

    public RegistrationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: REGISTRATIONS
    [HttpGet("")]
    public async Task<IActionResult> Index(ListRegDto? data)
    {
        if (data is null)
            data = new ListRegDto();

        data.registrations = await _context.Registrations
            .Include(r => r.Student)
            .Include(r => r.Course)
            .Select(r => new RegisterationDto()
            {
                Id = r.RegistrationId,
                Name = String.Concat(r.Student.FirstName, " ", r.Student.LastName),
                CourseId = r.CourseId,
                Course = r.Course.CourseName,
                Grade = r.Grade
            })
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(data.searchString))
        {
            data.registrations = data.registrations.Where(r =>
                r.Name.Contains(data.searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (data.SearchCourseId.HasValue)
        {
            data.registrations = data.registrations.Where(r =>
                r.CourseId == data.SearchCourseId.Value).ToList();
        }


        //if (courseId.HasValue)
        //{
        //    registrations = registrations.Where(r => r.CourseId == courseId.Value);
        //}

        data.CourseList = await GetCourseSelectListAsync();

        //foreach (var course in )
        //{
        //    course.Selected = courseId.HasValue && course.Value == courseId.Value.ToString();
        //}

        //ViewData["CurrentFilter"] = searchString;
        //ViewData["CurrentCourseId"] = courseId;
        //ViewData["Courses"] = courseList;

        //var data = new List<RegisterationDto>();

        //foreach (var r in registrations)
        //{
        //    data.Add(new RegisterationDto
        //    {
        //        Id = r.RegistrationId,
        //        Name = String.Concat(r.Student.FirstName, " ", r.Student.LastName),
        //        Course = r.Course.CourseName,
        //        Grade = r.Grade
        //    });
        //}

        return View(data);
    }

    // GET: REGISTRATIONS/Details/5
    [HttpGet("Details/{id?}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var registration = await _context.Registrations
            .Include(r => r.Student)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(m => m.RegistrationId == id);
        if (registration == null)
        {
            return NotFound();
        }

        return View(registration);
    }

    // GET: REGISTRATIONS/Create
    [HttpGet("Create")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateAsync()
    {
        var registration = new Registration
        {
            RegistrationDate = DateTime.Now,
            Status = RegistrationStatus.Active
        };

        var vm = new RegistrationCreateViewModel
        {
            Registration = registration,
            Students = await GetStudentSelectListAsync(),
            Courses = await GetCourseSelectListAsync()
        };

        return View(vm);
    }

    private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetStudentSelectListAsync()
    {
        var studentsData = await _context.Students
            .OrderBy(s => s.FirstName)
            .Select(s => new { s.StudentId, s.FirstName, s.LastName })
            .ToListAsync();

        return studentsData
            .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = s.StudentId.ToString(),
                Text = s.FirstName + " " + s.LastName
            })
            .ToList();
    }

    private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetCourseSelectListAsync()
    {
        var coursesData = await _context.Courses
            .OrderBy(c => c.CourseName)
            .Select(c => new { c.CourseId, c.CourseName })
            .ToListAsync();

        return coursesData
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.CourseId.ToString(),
                Text = c.CourseName
            })
            .ToList();
    }

    // GET: REGISTRATION/GetStudent/5
    [HttpGet("GetStudent/{studentid}")]
    public async Task<IActionResult> GetStudent(int studentid)
    {
        var student = await _context.Students.FindAsync(studentid);
        if (student == null)
            return NotFound();

        return Json(new { id = student.StudentId, name = student.FirstName + " " + student.LastName });
    }

    // GET: REGISTRATION/GetCourse/5
    [HttpGet("GetCourse/{courseid}")]
    public async Task<IActionResult> GetCourse(int courseid)
    {
        var course = await _context.Courses.FindAsync(courseid);
        if (course == null)
            return NotFound();

        return Json(new { id = course.CourseId, name = course.CourseName });
    }

    // POST: REGISTRATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create(RegistrationCreateViewModel vm)
    {
        var registration = vm?.Registration ?? new Registration();

        var studentExists = await _context.Students
            .AnyAsync(s => s.StudentId == registration.StudentId);

        if (!studentExists)
        {
            // Key must match form field name: Registration.StudentId
            ModelState.AddModelError("Registration.StudentId", "Please select a valid student.");
        }

        // Check that the selected course exists
        var courseExists = await _context.Courses
            .AnyAsync(c => c.CourseId == registration.CourseId);

        if (!courseExists)
        {
            ModelState.AddModelError("Registration.CourseId", "Please select a valid course.");
        }

        if (!ModelState.IsValid)
        {
            // Re-populate dropdown data and return view model
            vm = vm ?? new RegistrationCreateViewModel();
            vm.Students = await GetStudentSelectListAsync();
            vm.Courses = await GetCourseSelectListAsync();

            return View(vm);
        }

        registration.RegistrationDate =
            registration.RegistrationDate == default
                ? DateTime.Now
                : registration.RegistrationDate;

        _context.Registrations.Add(registration);

        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = "Registration created successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    // GET: REGISTRATIONS/Edit/5
    [HttpGet("Edit/{id?}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var registration = await _context.Registrations.FindAsync(id);
        if (registration == null)
        {
            return NotFound();
        }

        var vm = new RegistrationCreateViewModel
        {
            Registration = registration,
            Students = await GetStudentSelectListAsync(),
            Courses = await GetCourseSelectListAsync()
        };

        return View(vm);
    }

    // POST: REGISTRATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost("Edit/{id?}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int? id, RegistrationCreateViewModel vm)
    {
        var registration = vm?.Registration ?? new Registration();

        if (id != registration.RegistrationId)
        {
            return NotFound();
        }

        var studentExists = await _context.Students
            .AnyAsync(s => s.StudentId == registration.StudentId);

        if (!studentExists)
        {
            ModelState.AddModelError("Registration.StudentId", "Please select a valid student.");
        }

        var courseExists = await _context.Courses
            .AnyAsync(c => c.CourseId == registration.CourseId);

        if (!courseExists)
        {
            ModelState.AddModelError("Registration.CourseId", "Please select a valid course.");
        }

        if (!ModelState.IsValid)
        {
            vm = vm ?? new RegistrationCreateViewModel();
            vm.Students = await GetStudentSelectListAsync();
            vm.Courses = await GetCourseSelectListAsync();

            return View(vm);
        }

        try
        {
            _context.Update(registration);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RegistrationExists(registration.RegistrationId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        TempData["ToastMessage"] = "Registration updated successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    // GET: REGISTRATIONS/Delete/5
    [HttpGet("Delete/{id?}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var registration = await _context.Registrations
            .Include(r => r.Student)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(m => m.RegistrationId == id);
        if (registration == null)
        {
            return NotFound();
        }

        return View(registration);
    }

    // POST: REGISTRATIONS/Delete/5
    [HttpPost("Delete/{id?}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var registration = await _context.Registrations.FindAsync(id);
        if (registration != null)
        {
            _context.Registrations.Remove(registration);
        }

        await _context.SaveChangesAsync();
        TempData["ToastMessage"] = "Registration deleted successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    private bool RegistrationExists(int? id)
    {
        return _context.Registrations.Any(e => e.RegistrationId == id);
    }


}
