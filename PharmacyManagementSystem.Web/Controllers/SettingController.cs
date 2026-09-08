using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;
using System.Security.Claims;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        // ==========================================================
        // SETTINGS PAGE
        // ==========================================================

        [HttpGet]
        public IActionResult Index()
        {
            int? pharmacyId = GetPharmacyId();

            if (!pharmacyId.HasValue)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            PharmacySettings? settings =
                PharmacyStorage.GetById(
                    pharmacyId.Value);

            if (settings == null)
            {
                TempData["ErrorMessage"] =
                    "Pharmacy information could not be found.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View(settings);
        }


        // ==========================================================
        // UPDATE PHARMACY SETTINGS
        // ADMIN ONLY
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePharmacy(
            PharmacySettings settings)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            int? pharmacyId = GetPharmacyId();

            if (!pharmacyId.HasValue)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (settings == null)
            {
                TempData["ErrorMessage"] =
                    "Invalid pharmacy information.";

                return RedirectToAction(
                    nameof(Index));
            }


            // ======================================================
            // NEVER TRUST PHARMACY ID FROM BROWSER
            // ======================================================

            settings.PharmacyId =
                pharmacyId.Value;


            // ======================================================
            // PHARMACY NAME
            // ======================================================

            if (string.IsNullOrWhiteSpace(
                    settings.PharmacyName))
            {
                TempData["ErrorMessage"] =
                    "Pharmacy name is required.";

                return RedirectToAction(
                    nameof(Index));
            }


            settings.PharmacyName =
                settings.PharmacyName.Trim();


            // ======================================================
            // PHONE
            // ======================================================

            settings.Phone =
                settings.Phone?.Trim() ?? "";


            // ======================================================
            // ADDRESS
            // ======================================================

            settings.Address =
                settings.Address?.Trim() ?? "";


            // ======================================================
            // CURRENCY
            // ======================================================

            string[] allowedCurrencies =
            {
                "Rs.",
                "$",
                "€",
                "£"
            };

            if (!allowedCurrencies.Contains(
                    settings.Currency))
            {
                settings.Currency = "Rs.";
            }


            // ======================================================
            // LOW STOCK THRESHOLD
            // ======================================================

            if (settings.LowStockThreshold < 1)
            {
                settings.LowStockThreshold = 10;
            }

            if (settings.LowStockThreshold > 10000)
            {
                settings.LowStockThreshold = 10000;
            }


            // ======================================================
            // DATE FORMAT
            // ======================================================

            string[] allowedDateFormats =
            {
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy-MM-dd"
            };

            if (!allowedDateFormats.Contains(
                    settings.DateFormat))
            {
                settings.DateFormat =
                    "dd/MM/yyyy";
            }


            // ======================================================
            // SAVE
            // ======================================================

            bool success =
                PharmacyStorage.Update(
                    settings);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to update pharmacy information.";

                return RedirectToAction(
                    nameof(Index));
            }


            TempData["SuccessMessage"] =
                "Pharmacy settings updated successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ==========================================================
        // CHANGE PASSWORD - GET
        // ==========================================================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        // ==========================================================
        // CHANGE PASSWORD - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            string? pharmacyClaim =
                User.FindFirstValue(
                    "PharmacyId");


            if (!int.TryParse(
                    userId,
                    out int parsedUserId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!int.TryParse(
                    pharmacyClaim,
                    out int parsedPharmacyId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // ======================================================
            // CURRENT PASSWORD
            // ======================================================

            if (string.IsNullOrWhiteSpace(
                    currentPassword))
            {
                TempData["ErrorMessage"] =
                    "Current password is required.";

                return View();
            }


            // ======================================================
            // NEW PASSWORD
            // ======================================================

            if (string.IsNullOrWhiteSpace(
                    newPassword))
            {
                TempData["ErrorMessage"] =
                    "New password is required.";

                return View();
            }


            // ======================================================
            // PASSWORD LENGTH
            // ======================================================

            if (newPassword.Length < 8)
            {
                TempData["ErrorMessage"] =
                    "New password must contain at least 8 characters.";

                return View();
            }


            // ======================================================
            // PASSWORD STRENGTH
            // ======================================================

            if (!IsStrongPassword(newPassword))
            {
                TempData["ErrorMessage"] =
                    "Password must contain uppercase, lowercase, number and special character.";

                return View();
            }


            // ======================================================
            // CONFIRM PASSWORD
            // ======================================================

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] =
                    "New passwords do not match.";

                return View();
            }


            // ======================================================
            // NEW PASSWORD MUST BE DIFFERENT
            // ======================================================

            if (currentPassword == newPassword)
            {
                TempData["ErrorMessage"] =
                    "New password must be different from your current password.";

                return View();
            }


            // ======================================================
            // GET CURRENT USER
            // PHARMACY SCOPED
            // ======================================================

            User? user =
                UserStorage.GetByIdForPharmacy(
                    parsedUserId,
                    parsedPharmacyId);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "User account could not be found.";

                return View();
            }


            // ======================================================
            // VERIFY CURRENT PASSWORD
            // ======================================================

            bool currentPasswordValid =
                UserStorage.VerifyPassword(
                    user,
                    currentPassword);

            if (!currentPasswordValid)
            {
                TempData["ErrorMessage"] =
                    "Current password is incorrect.";

                return View();
            }


            // ======================================================
            // CHANGE PASSWORD
            // PHARMACY SCOPED
            // ======================================================

            bool success =
                UserStorage.ChangePassword(
                    parsedUserId,
                    parsedPharmacyId,
                    newPassword);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to change password.";

                return View();
            }


            TempData["SuccessMessage"] =
                "Password changed successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ==========================================================
        // MANAGE USERS
        // ==========================================================

        [HttpGet]
        public IActionResult ManageUsers()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            return RedirectToAction(
                "Index",
                "UserManagement");
        }


        // ==========================================================
        // GET PHARMACY ID
        // ==========================================================

        private int? GetPharmacyId()
        {
            string? pharmacyId =
                User.FindFirstValue(
                    "PharmacyId");

            if (!int.TryParse(
                    pharmacyId,
                    out int parsedPharmacyId))
            {
                return null;
            }

            if (parsedPharmacyId <= 0)
            {
                return null;
            }

            return parsedPharmacyId;
        }


        // ==========================================================
        // CHECK ADMIN
        // ==========================================================

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }


        // ==========================================================
        // STRONG PASSWORD
        // ==========================================================

        private bool IsStrongPassword(
            string password)
        {
            bool hasUppercase =
                password.Any(
                    char.IsUpper);

            bool hasLowercase =
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

            return hasUppercase &&
                   hasLowercase &&
                   hasNumber &&
                   hasSpecial;
        }
    }
}