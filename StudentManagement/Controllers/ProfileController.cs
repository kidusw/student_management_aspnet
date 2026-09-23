using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

[Authorize]
public class ProfileController : Controller
{
    private static readonly Dictionary<string, string> AllowedImageTypes = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    // GET: PROFILE
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            CurrentProfilePicturePath = user.ProfilePicturePath
        };

        return View(model);
    }

    // POST: PROFILE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        if (model.ProfilePicture != null)
        {
            if (!AllowedImageTypes.TryGetValue(model.ProfilePicture.ContentType, out var extension))
            {
                ModelState.AddModelError(nameof(model.ProfilePicture), "Please upload a JPEG, PNG, GIF, or WEBP image.");
            }
            else if (model.ProfilePicture.Length > MaxFileSizeBytes)
            {
                ModelState.AddModelError(nameof(model.ProfilePicture), "The image must be 2 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.UserName = user.UserName ?? string.Empty;
            model.CurrentProfilePicturePath = user.ProfilePicturePath;
            return View(model);
        }

        user.DisplayName = model.DisplayName;

        if (model.ProfilePicture != null)
        {
            var extension = AllowedImageTypes[model.ProfilePicture.ContentType];
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfilePicture.CopyToAsync(stream);
            }

            var previousPicturePath = user.ProfilePicturePath;
            user.ProfilePicturePath = $"/uploads/{fileName}";

            if (!string.IsNullOrEmpty(previousPicturePath))
            {
                var previousFilePath = Path.Combine(_environment.WebRootPath, previousPicturePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(previousFilePath))
                {
                    System.IO.File.Delete(previousFilePath);
                }
            }
        }

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        TempData["ToastMessage"] = "Profile updated successfully.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }
}
