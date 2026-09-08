using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PharmacyManagementSystem.Web.Controllers
{
    // ============================================================
    // PHARMACY BASE CONTROLLER
    // ============================================================
    //
    // Every protected pharmacy controller should inherit from this.
    //
    // The current pharmacy is taken from the authenticated user's
    // PharmacyId claim.
    //
    // Database queries MUST use CurrentPharmacyId.
    //
    // ============================================================

    [Authorize]
    public abstract class PharmacyControllerBase : Controller
    {
        // ============================================================
        // CURRENT USER ID
        // ============================================================

        protected int CurrentUserId
        {
            get
            {
                string? value =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (int.TryParse(
                    value,
                    out int userId))
                {
                    return userId;
                }

                return 0;
            }
        }


        // ============================================================
        // CURRENT PHARMACY ID
        // ============================================================

        protected int CurrentPharmacyId
        {
            get
            {
                string? value =
                    User.FindFirstValue(
                        "PharmacyId");

                if (int.TryParse(
                    value,
                    out int pharmacyId))
                {
                    return pharmacyId;
                }

                return 0;
            }
        }


        // ============================================================
        // CURRENT USERNAME
        // ============================================================

        protected string CurrentUsername
        {
            get
            {
                return
                    User.FindFirstValue(
                        ClaimTypes.Name)
                    ?? "";
            }
        }


        // ============================================================
        // CURRENT ROLE
        // ============================================================

        protected string CurrentRole
        {
            get
            {
                return
                    User.FindFirstValue(
                        ClaimTypes.Role)
                    ?? "";
            }
        }


        // ============================================================
        // CURRENT PHARMACY NAME
        // ============================================================

        protected string CurrentPharmacyName
        {
            get
            {
                return
                    User.FindFirstValue(
                        "PharmacyName")
                    ?? "";
            }
        }


        // ============================================================
        // CHECK VALID PHARMACY
        // ============================================================

        protected bool HasValidPharmacy()
        {
            return CurrentPharmacyId > 0;
        }


        // ============================================================
        // REQUIRE VALID PHARMACY
        // ============================================================

        protected IActionResult? RequirePharmacy()
        {
            if (CurrentPharmacyId <= 0)
            {
                return RedirectToAction(
                    "Logout",
                    "Account");
            }

            return null;
        }
    }
}