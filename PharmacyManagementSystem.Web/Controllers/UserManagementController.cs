using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;
using System.Security.Claims;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private static readonly List<string> AvailablePermissions =
            new List<string>
            {
                "Medicine Management",
                "Stock Management",
                "Sales Management",
                "Reports"
            };


        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public IActionResult Index()
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            var users =
                UserStorage.GetByPharmacyId(
                    currentUser.PharmacyId);


            return View(users);
        }


        // ============================================================
        // ADD - GET
        // ============================================================

        [HttpGet]
        public IActionResult Add()
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            var user = new User
            {
                PharmacyId =
                    currentUser.PharmacyId,

                Role = "Staff",

                IsActive = true
            };


            return View(user);
        }


        // ============================================================
        // ADD - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(
            User user,
            string password,
            string confirmPassword)
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            // Never trust PharmacyId submitted by the browser.
            user.PharmacyId =
                currentUser.PharmacyId;


            // New users created through this screen
            // are Staff accounts.
            user.Role = "Staff";

            user.IsActive = true;


            if (string.IsNullOrWhiteSpace(
                    user.Username))
            {
                TempData["ErrorMessage"] =
                    "Username is required.";

                return View(user);
            }


            user.Username =
                user.Username.Trim();


            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] =
                    "Password is required.";

                return View(user);
            }


            if (!IsStrongPassword(password))
            {
                TempData["ErrorMessage"] =
                    "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.";

                return View(user);
            }


            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] =
                    "Passwords do not match.";

                return View(user);
            }


            // Username is globally UNIQUE in the database.
            var existingUser =
                UserStorage.GetByUsername(
                    user.Username);


            if (existingUser != null)
            {
                TempData["ErrorMessage"] =
                    "This username is already in use. Please choose another username.";

                return View(user);
            }


            var success =
                UserStorage.Add(
                    user,
                    password);


            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to create the user. The username may already exist.";

                return View(user);
            }


            TempData["SuccessMessage"] =
                $"User '{user.Username}' was created successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // PERMISSIONS - GET
        // ============================================================

        [HttpGet]
        public IActionResult Permissions(int id)
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            // IMPORTANT:
            // Pharmacy restriction happens inside SQL.
            var targetUser =
                UserStorage.GetByIdForPharmacy(
                    id,
                    currentUser.PharmacyId);


            if (targetUser == null)
            {
                return NotFound();
            }


            // Admin always has full access.
            // Their permissions must not be manually changed.
            if (IsAdmin(targetUser))
            {
                TempData["ErrorMessage"] =
                    "Administrator permissions cannot be changed.";

                return RedirectToAction(
                    nameof(Index));
            }


            targetUser.Permissions =
                UserStorage.GetPermissionsForPharmacy(
                    targetUser.Id,
                    currentUser.PharmacyId);


            ViewBag.AvailablePermissions =
                AvailablePermissions;


            return View(targetUser);
        }


        // ============================================================
        // PERMISSIONS - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Permissions(
            int id,
            List<string>? permissions)
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            // IMPORTANT:
            // SQL checks both user ID and pharmacy ID.
            var targetUser =
                UserStorage.GetByIdForPharmacy(
                    id,
                    currentUser.PharmacyId);


            if (targetUser == null)
            {
                return NotFound();
            }


            if (IsAdmin(targetUser))
            {
                TempData["ErrorMessage"] =
                    "Administrator permissions cannot be changed.";

                return RedirectToAction(
                    nameof(Index));
            }


            permissions ??=
                new List<string>();


            // Only allow permissions defined by the application.
            var validPermissions =
                permissions
                    .Where(permission =>
                        AvailablePermissions.Contains(
                            permission,
                            StringComparer.OrdinalIgnoreCase))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();


            var success =
                UserStorage.SetPermissions(
                    targetUser.Id,
                    currentUser.PharmacyId,
                    validPermissions);


            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to update user permissions.";

                return RedirectToAction(
                    nameof(Index));
            }


            TempData["SuccessMessage"] =
                $"Permissions for '{targetUser.Username}' were updated successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            // Pharmacy is checked in SQL.
            var targetUser =
                UserStorage.GetByIdForPharmacy(
                    id,
                    currentUser.PharmacyId);


            if (targetUser == null)
            {
                return NotFound();
            }


            if (targetUser.Id ==
                currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You cannot delete your own account.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (IsAdmin(targetUser))
            {
                TempData["ErrorMessage"] =
                    "Administrator accounts cannot be deleted.";

                return RedirectToAction(
                    nameof(Index));
            }


            var success =
                UserStorage.Delete(
                    targetUser.Id,
                    currentUser.PharmacyId);


            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to delete the user.";

                return RedirectToAction(
                    nameof(Index));
            }


            TempData["SuccessMessage"] =
                $"User '{targetUser.Username}' was deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // TOGGLE STATUS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var currentUser =
                GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!IsAdmin(currentUser))
            {
                return Forbid();
            }


            // Pharmacy is checked in SQL.
            var targetUser =
                UserStorage.GetByIdForPharmacy(
                    id,
                    currentUser.PharmacyId);


            if (targetUser == null)
            {
                return NotFound();
            }


            if (targetUser.Id ==
                currentUser.Id)
            {
                TempData["ErrorMessage"] =
                    "You cannot change your own account status.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (IsAdmin(targetUser))
            {
                TempData["ErrorMessage"] =
                    "Administrator account status cannot be changed.";

                return RedirectToAction(
                    nameof(Index));
            }


            var newStatus =
                !targetUser.IsActive;


            var success =
                UserStorage.SetActiveStatus(
                    targetUser.Id,
                    currentUser.PharmacyId,
                    newStatus);


            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to update the user's status.";

                return RedirectToAction(
                    nameof(Index));
            }


            TempData["SuccessMessage"] =
                newStatus
                    ? $"User '{targetUser.Username}' has been activated."
                    : $"User '{targetUser.Username}' has been deactivated.";


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // GET CURRENT USER
        // ============================================================

        private User? GetCurrentUser()
        {
            var claim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier);


            if (claim == null)
                return null;


            if (!int.TryParse(
                    claim.Value,
                    out int userId))
            {
                return null;
            }


            // Obtain pharmacy from the authenticated user's claim.
            var pharmacyClaim =
                User.FindFirst("PharmacyId");


            if (pharmacyClaim == null)
                return null;


            if (!int.TryParse(
                    pharmacyClaim.Value,
                    out int pharmacyId))
            {
                return null;
            }


            // IMPORTANT:
            // Both ID and pharmacy ID are checked by SQL.
            return UserStorage.GetByIdForPharmacy(
                userId,
                pharmacyId);
        }


        // ============================================================
        // ADMIN CHECK
        // ============================================================

        private static bool IsAdmin(User user)
        {
            return string.Equals(
                user.Role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // STRONG PASSWORD CHECK
        // ============================================================

        private static bool IsStrongPassword(
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;


            if (password.Length < 8)
                return false;


            bool hasUpper =
                password.Any(char.IsUpper);

            bool hasLower =
                password.Any(char.IsLower);

            bool hasNumber =
                password.Any(char.IsDigit);

            bool hasSpecial =
                password.Any(
                    character =>
                        !char.IsLetterOrDigit(character));


            return hasUpper
                   && hasLower
                   && hasNumber
                   && hasSpecial;
        }
    }
}