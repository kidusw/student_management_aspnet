
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Data;

[Route("Registration")]
public class RegistrationController : Controller
{
    private readonly ApplicationDbContext _context;

    public RegistrationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: REGISTRATIONS
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Registrations.ToListAsync());
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
            .FirstOrDefaultAsync(m => m.RegistrationId == id);
        if (registration == null)
        {
            return NotFound();
        }

        return View(registration);
    }

    // GET: REGISTRATIONS/Create
    [HttpGet("Create")]
    public async Task<IActionResult> CreateAsync()
    {
        var registration = new Registration
        {
            RegistrationDate = DateTime.Now,
            Status = "Active"
        };

        // Load minimal student/course data then project to SelectListItem in memory
        var studentsData = await _context.Students
            .OrderBy(s => s.FirstName)
            .Select(s => new { s.StudentId, s.FirstName, s.LastName })
            .ToListAsync();

        var coursesData = await _context.Courses
            .OrderBy(c => c.CourseName)
            .Select(c => new { c.CourseId, c.CourseName })
            .ToListAsync();

        var vm = new RegistrationCreateViewModel
        {
            Registration = registration,
            Students = studentsData
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = s.FirstName + " " + s.LastName
                })
                .ToList(),
            Courses = coursesData
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                })
                .ToList()
        };

        return View(vm);
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
            var studentsData = await _context.Students
                .OrderBy(s => s.FirstName)
                .Select(s => new { s.StudentId, s.FirstName, s.LastName })
                .ToListAsync();

            var coursesData = await _context.Courses
                .OrderBy(c => c.CourseName)
                .Select(c => new { c.CourseId, c.CourseName })
                .ToListAsync();

            vm = vm ?? new RegistrationCreateViewModel();
            vm.Students = studentsData
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = s.FirstName + " " + s.LastName
                })
                .ToList();
            vm.Courses = coursesData
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                })
                .ToList();

            return View(vm);
        }

        registration.RegistrationDate =
            registration.RegistrationDate == default
                ? DateTime.Now
                : registration.RegistrationDate;

        _context.Registrations.Add(registration);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: REGISTRATIONS/Edit/5
    [HttpGet("Edit/{id?}")]
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
        return View(registration);
    }

    // POST: REGISTRATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost("Edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("RegistrationId,StudentId,CourseId,RegistrationDate,Grade,Status,Student,Course")] Registration registration)
    {
        if (id != registration.RegistrationId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
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
            return RedirectToAction(nameof(Index));
        }
        return View(registration);
    }

    // GET: REGISTRATIONS/Delete/5
    [HttpGet("Delete/{id?}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var registration = await _context.Registrations
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
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var registration = await _context.Registrations.FindAsync(id);
        if (registration != null)
        {
            _context.Registrations.Remove(registration);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RegistrationExists(int? id)
    {
        return _context.Registrations.Any(e => e.RegistrationId == id);
    }


}
