using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopDomain.Models;
using System.Threading.Tasks;

namespace ShopInfrastructure.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                BirthDate = user.BirthDate,
                ShippingAddress = user.ShippingAddress,
                Roles = await _userManager.GetRolesAsync(user)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                BirthDate = user.BirthDate,
                ShippingAddress = user.ShippingAddress,
                Roles = await _userManager.GetRolesAsync(user) // Для відображення
            };

            ViewBag.Countries = new List<string> { "Україна", "Польща", "Німеччина", "США" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = new List<string> { "Україна", "Польща", "Німеччина", "США" };
                model.Roles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User)); // Додаємо Roles для відображення
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Оновлюємо тільки редаговані поля
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Country = model.Country;
            user.BirthDate = model.BirthDate;
            user.ShippingAddress = model.ShippingAddress;
            // Email і Roles не оновлюємо!

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Профіль успішно оновлено!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Countries = new List<string> { "Україна", "Польща", "Німеччина", "США" };
            model.Roles = await _userManager.GetRolesAsync(user); // Додаємо Roles для відображення при помилці
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Пароль успішно змінено!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
}