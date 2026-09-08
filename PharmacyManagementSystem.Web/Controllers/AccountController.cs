using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;
using System.Security.Claims;

namespace PharmacyManagementSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        // ============================================================
        // LOGIN - GET
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View();
        }


        // ============================================================
        // LOGIN - POST
        // ============================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError(
                    "",
                    "Username is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }


            // ========================================================
            // FIND USER
            // ========================================================

            User? user =
                UserStorage.GetByUsername(
                    username.Trim());


            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View();
            }


            // ========================================================
            // CHECK ACTIVE STATUS
            // ========================================================

            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "Your account has been deactivated. Please contact the administrator.");

                return View();
            }


            // ========================================================
            // VERIFY PASSWORD
            // ========================================================

            bool passwordValid =
                UserStorage.VerifyPassword(
                    user,
                    password);


            if (!passwordValid)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View();
            }


            // ========================================================
            // LOAD PHARMACY
            // ========================================================

            PharmacySettings? pharmacy =
                PharmacyStorage.GetById(
                    user.PharmacyId);


            if (pharmacy == null)
            {
                ModelState.AddModelError(
                    "",
                    "The pharmacy associated with this account could not be found.");

                return View();
            }


            // ========================================================
            // UPDATE USER PHARMACY NAME
            // ========================================================

            user.PharmacyName =
                pharmacy.PharmacyName;


            // ========================================================
            // LOAD PERMISSIONS
            // ========================================================

            user.Permissions =
                UserStorage.GetPermissions(
                    user.Id);


            // ========================================================
            // CREATE AUTHENTICATION CLAIMS
            // ========================================================

            List<Claim> claims =
                new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.Id.ToString()),

                    new Claim(
                        ClaimTypes.Name,
                        user.Username),

                    new Claim(
                        ClaimTypes.Role,
                        user.Role),

                    new Claim(
                        "PharmacyId",
                        user.PharmacyId.ToString()),

                    new Claim(
                        "PharmacyName",
                        user.PharmacyName ?? "")
                };


            // ========================================================
            // ADD PERMISSION CLAIMS
            // ========================================================

            foreach (string permission
                in user.Permissions)
            {
                if (string.IsNullOrWhiteSpace(permission))
                {
                    continue;
                }

                claims.Add(
                    new Claim(
                        "Permission",
                        permission));
            }


            // ========================================================
            // CREATE IDENTITY
            // ========================================================

            ClaimsIdentity identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);


            ClaimsPrincipal principal =
                new ClaimsPrincipal(
                    identity);


            // ========================================================
            // SIGN IN
            // ========================================================

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal);


            // ========================================================
            // GO TO DASHBOARD
            // ========================================================

            return RedirectToAction(
                "Index",
                "Home");
        }


        // ============================================================
        // REGISTER PHARMACY - GET
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View(
                new RegisterPharmacyViewModel());
        }


        // ============================================================
        // REGISTER PHARMACY - POST
        // ============================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(
            RegisterPharmacyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ========================================================
            // PASSWORD STRENGTH
            // ========================================================

            if (!HasStrongPassword(
                    model.Password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password must contain uppercase, lowercase, number and special character.");

                return View(model);
            }


            // ========================================================
            // CHECK USERNAME
            // ========================================================

            User? existingUser =
                UserStorage.GetByUsername(
                    model.AdminUsername.Trim());


            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "AdminUsername",
                    "This username is already in use.");

                return View(model);
            }


            // ========================================================
            // CREATE PHARMACY OBJECT
            // ========================================================

            PharmacySettings pharmacy =
                new PharmacySettings
                {
                    PharmacyName =
                        model.PharmacyName.Trim(),

                    Phone =
                        model.Phone?.Trim() ?? "",

                    Address =
                        model.Address?.Trim() ?? "",

                    Currency =
                        "Rs.",

                    LowStockThreshold =
                        10,

                    DateFormat =
                        "dd/MM/yyyy"
                };


            // ========================================================
            // REGISTER PHARMACY + ADMIN
            // ========================================================

            int pharmacyId =
                PharmacyRegistrationStorage
                    .RegisterPharmacy(
                        pharmacy,
                        model.AdminUsername.Trim(),
                        model.Password);


            if (pharmacyId <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to register pharmacy. Please try again.");

                return View(model);
            }


            // ========================================================
            // SUCCESS
            // ========================================================

            TempData["SuccessMessage"] =
                "Pharmacy registered successfully. You can now login.";


            return RedirectToAction(
                nameof(Login));
        }


        // ============================================================
        // LOGOUT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return RedirectToAction(
                nameof(Login));
        }


        // ============================================================
        // ACCESS DENIED
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // ============================================================
        // PASSWORD STRENGTH
        // ============================================================

        private bool HasStrongPassword(
            string password)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8)
            {
                return false;
            }


            bool hasUpper =
                password.Any(
                    char.IsUpper);


            bool hasLower =
                password.Any(
                    char.IsLower);


            bool hasNumber =
                password.Any(
                    char.IsDigit);


            bool hasSpecial =
                password.Any(
                    character =>
                        !char.IsLetterOrDigit(
                            character));


            return
                hasUpper &&
                hasLower &&
                hasNumber &&
                hasSpecial;
        }
    }
}