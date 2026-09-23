using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // GET: ACCOUNT/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: ACCOUNT/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserNameOrEmail)
            ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account has been deactivated.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            TempData["ToastMessage"] = "Logged in successfully.";
            TempData["ToastType"] = "success";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked out. Please try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    // POST: ACCOUNT/Logout
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["ToastMessage"] = "Logged out successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Login));
    }

    // GET: ACCOUNT/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
