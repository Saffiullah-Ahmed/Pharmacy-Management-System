using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmacyManagementSystem.Web.Controllers
{
    public class SetupController : Controller
    {
        // ============================================================
        // OLD CREATE ADMIN URL
        // ============================================================
        // Pharmacy registration is now handled through:
        //
        // /Account/Register
        //
        // This action only redirects old links.
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateAdmin()
        {
            return RedirectToAction(
                "Register",
                "Account");
        }


        // ============================================================
        // OLD CREATE ADMIN POST
        // ============================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAdmin(
            string username,
            string password,
            string confirmPassword)
        {
            return RedirectToAction(
                "Register",
                "Account");
        }
    }
}