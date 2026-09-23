
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Data;

[Authorize(Roles = "Admin,Staff,Viewer")]
public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;

    public CourseController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: COURSES
    public async Task<IActionResult> Index()
    {
        return View(await _context.Courses.Include(c => c.Department).ToListAsync());
    }

    // GET: COURSES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Department)
            .FirstOrDefaultAsync(m => m.CourseId == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // GET: COURSES/Create
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create()
    {
        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View();
    }

    // POST: COURSES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([Bind("CourseId,CourseCode,CourseName,Description,CreditHours,DepartmentId,Registrations")] Course course)
    {
        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == course.DepartmentId);

        if (!departmentExists)
        {
            ModelState.AddModelError("DepartmentId", "Please select a valid department.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(course);
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Course created successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View(course);
    }

    // GET: COURSES/Edit/5
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View(course);
    }

    // POST: COURSES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int? id, [Bind("CourseId,CourseCode,CourseName,Description,CreditHours,DepartmentId,Registrations")] Course course)
    {
        if (id != course.CourseId)
        {
            return NotFound();
        }

        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == course.DepartmentId);

        if (!departmentExists)
        {
            ModelState.AddModelError("DepartmentId", "Please select a valid department.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.CourseId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["ToastMessage"] = "Course updated successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View(course);
    }

    // GET: COURSES/Delete/5
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Department)
            .FirstOrDefaultAsync(m => m.CourseId == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: COURSES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
        }

        await _context.SaveChangesAsync();
        TempData["ToastMessage"] = "Course deleted successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int? id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }

    private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetDepartmentSelectListAsync()
    {
        var departmentsData = await _context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new { d.DepartmentId, d.Name })
            .ToListAsync();

        return departmentsData
            .Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name
            })
            .ToList();
    }
}
