using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class MedicineController : PharmacyControllerBase
    {
        // ============================================================
        // MEDICINE LIST
        // ============================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            int pharmacyId = GetCurrentPharmacyId();

            if (pharmacyId <= 0)
            {
                return Forbid();
            }

            List<Medicine> medicines =
                MedicineStorage.Load(pharmacyId);

            return View(medicines);
        }


        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            if (GetCurrentPharmacyId() <= 0)
            {
                return Forbid();
            }

            return View();
        }


        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Medicine medicine)
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            int pharmacyId = GetCurrentPharmacyId();

            if (pharmacyId <= 0)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(medicine);
            }

            bool success =
                MedicineStorage.Add(
                    medicine,
                    pharmacyId);

            if (!success)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to add medicine.");

                return View(medicine);
            }

            TempData["SuccessMessage"] =
                "Medicine added successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            int pharmacyId = GetCurrentPharmacyId();

            if (pharmacyId <= 0)
            {
                return Forbid();
            }

            Medicine? medicine =
                MedicineStorage.GetById(
                    id,
                    pharmacyId);

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Medicine medicine)
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            int pharmacyId = GetCurrentPharmacyId();

            if (pharmacyId <= 0)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(medicine);
            }

            bool success =
                MedicineStorage.Update(
                    medicine,
                    pharmacyId);

            if (!success)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update medicine.");

                return View(medicine);
            }

            TempData["SuccessMessage"] =
                "Medicine updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DELETE - GET
        // ============================================================

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            int pharmacyId = GetCurrentPharmacyId();

            if (pharmacyId <= 0)
            {
                return Forbid();
            }

            Medicine? medicine =
                MedicineStorage.GetById(
                    id,
                    pharmacyId);

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }


        // ============================================================
        // DELETE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!HasMedicinePermission())
            {
                return Forbid();
            }

            int pharmacyId = GetCurrentPharmacyId();

            if (pharmacyId <= 0)
            {
                return Forbid();
            }

            if (id <= 0)
            {
                TempData["ErrorMessage"] =
                    "Invalid medicine.";

                return RedirectToAction(nameof(Index));
            }

            bool success =
                MedicineStorage.Delete(
                    id,
                    pharmacyId);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Unable to delete medicine.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Medicine deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // MEDICINE PERMISSION
        // ============================================================

        private bool HasMedicinePermission()
        {
            return PermissionChecker.HasPermission(
                User,
                "Medicine Management");
        }


        // ============================================================
        // GET CURRENT PHARMACY ID
        // ============================================================

        private int GetCurrentPharmacyId()
        {
            string? pharmacyId =
                User.FindFirst("PharmacyId")?.Value;

            if (string.IsNullOrWhiteSpace(pharmacyId))
            {
                return 0;
            }

            if (!int.TryParse(
                    pharmacyId,
                    out int id))
            {
                return 0;
            }

            return id;
        }
    }
}