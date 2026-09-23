using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentManagement.Models;

[Authorize(Roles = Roles.Admin)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: USERS
    public async Task<IActionResult> Index()
    {
        var users = new List<UserListItemViewModel>();

        foreach (var user in _userManager.Users.OrderBy(u => u.DisplayName).ToList())
        {
            var roles = await _userManager.GetRolesAsync(user);
            users.Add(new UserListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive
            });
        }

        return View(users);
    }

    // GET: USERS/Create
    public IActionResult Create()
    {
        ViewData["Roles"] = GetRoleSelectList();
        return View();
    }

    // POST: USERS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
                TempData["ToastMessage"] = "User created successfully.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewData["Roles"] = GetRoleSelectList();
        return View(model);
    }

    // GET: USERS/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            SelectedRole = roles.FirstOrDefault() ?? Roles.Staff,
            IsActive = user.IsActive
        };

        ViewData["Roles"] = GetRoleSelectList();
        return View(model);
    }

    // POST: USERS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var isSelf = string.Equals(id, _userManager.GetUserId(User), StringComparison.Ordinal);
        if (isSelf && (!model.IsActive || model.SelectedRole != Roles.Admin))
        {
            ModelState.AddModelError(string.Empty, "You cannot deactivate your own account or remove your own Admin role.");
        }

        if (ModelState.IsValid)
        {
            user.DisplayName = model.DisplayName;
            user.IsActive = model.IsActive;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.SelectedRole))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            TempData["ToastMessage"] = "User updated successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        model.UserName = user.UserName ?? string.Empty;
        ViewData["Roles"] = GetRoleSelectList();
        return View(model);
    }

    private static List<SelectListItem> GetRoleSelectList()
    {
        return Roles.All
            .Select(r => new SelectListItem { Value = r, Text = r })
            .ToList();
    }
}
