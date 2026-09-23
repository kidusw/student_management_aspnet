
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Data;

public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: STUDENTS
    public async Task<IActionResult> Index(string searchString)
    {
        var students = _context.Students.Include(s => s.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            students = students.Where(s =>
                s.FirstName.Contains(searchString) ||
                s.LastName.Contains(searchString) ||
                (s.FirstName + " " + s.LastName).Contains(searchString));
        }

        ViewData["CurrentFilter"] = searchString;

        return View(await students.ToListAsync());
    }

    // GET: STUDENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.Students
            .Include(s => s.Department)
            .Include(s => s.Registrations)
                .ThenInclude(r => r.Course)
            .FirstOrDefaultAsync(m => m.StudentId == id);
        if (student == null)
        {
            return NotFound();
        }

        return View(student);
    }

    // GET: STUDENTS/Create
    public async Task<IActionResult> Create()
    {
        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View();
    }

    // POST: STUDENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentId,FirstName,LastName,Age,DateOfBirth,MobileNumber,DepartmentId")] Student student)
    {
        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == student.DepartmentId);

        if (!departmentExists)
        {
            ModelState.AddModelError("DepartmentId", "Please select a valid department.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(student);
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Student created successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View(student);
    }

    // GET: STUDENTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View(student);
    }

    // POST: STUDENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("StudentId,FirstName,LastName,Age,DateOfBirth,MobileNumber,DepartmentId")] Student student)
    {
        if (id != student.StudentId)
        {
            return NotFound();
        }

        var departmentExists = await _context.Departments
            .AnyAsync(d => d.DepartmentId == student.DepartmentId);

        if (!departmentExists)
        {
            ModelState.AddModelError("DepartmentId", "Please select a valid department.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.StudentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["ToastMessage"] = "Student updated successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Departments"] = await GetDepartmentSelectListAsync();
        return View(student);
    }

    // GET: STUDENTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var student = await _context.Students
            .Include(s => s.Department)
            .FirstOrDefaultAsync(m => m.StudentId == id);
        if (student == null)
        {
            return NotFound();
        }

        return View(student);
    }

    // POST: STUDENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
        }

        await _context.SaveChangesAsync();
        TempData["ToastMessage"] = "Student deleted successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    private bool StudentExists(int? id)
    {
        return _context.Students.Any(e => e.StudentId == id);
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
