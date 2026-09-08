using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class SupplierController : PharmacyControllerBase
    {
        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            int pharmacyId = CurrentPharmacyId;

            var suppliers =
                SupplierStorage.Load(
                    pharmacyId
                );

            return View(suppliers);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            return View(new Supplier());
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Supplier supplier)
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            int pharmacyId = CurrentPharmacyId;


            // -----------------------------------------------------
            // CLEAN INPUT
            // -----------------------------------------------------

            supplier.Name =
                supplier.Name?.Trim() ?? string.Empty;

            supplier.Phone =
                supplier.Phone?.Trim() ?? string.Empty;

            supplier.Email =
                supplier.Email?.Trim() ?? string.Empty;

            supplier.Address =
                supplier.Address?.Trim() ?? string.Empty;

            supplier.Company =
                supplier.Company?.Trim() ?? string.Empty;


            // -----------------------------------------------------
            // REQUIRED VALIDATION
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(supplier.Name))
            {
                ModelState.AddModelError(
                    "Name",
                    "Supplier name is required."
                );
            }

            if (string.IsNullOrWhiteSpace(supplier.Phone))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Phone number is required."
                );
            }


            // -----------------------------------------------------
            // DUPLICATE SUPPLIER CHECK
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(supplier.Name))
            {
                var existingSupplier =
                    SupplierStorage.GetByName(
                        supplier.Name,
                        pharmacyId
                    );

                if (existingSupplier != null)
                {
                    ModelState.AddModelError(
                        "Name",
                        "A supplier with this name already exists in your pharmacy."
                    );
                }
            }


            // -----------------------------------------------------
            // SAVE
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                bool success =
                    SupplierStorage.Add(
                        supplier,
                        pharmacyId
                    );

                if (success)
                {
                    TempData["SuccessMessage"] =
                        "Supplier added successfully.";

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    "",
                    "Unable to add supplier. Please try again."
                );
            }

            return View(supplier);
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            int pharmacyId = CurrentPharmacyId;

            var supplier =
                SupplierStorage.GetById(
                    id,
                    pharmacyId
                );

            if (supplier == null)
            {
                TempData["ErrorMessage"] =
                    "Supplier not found or you do not have access to it.";

                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Supplier supplier)
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            int pharmacyId = CurrentPharmacyId;


            // -----------------------------------------------------
            // VERIFY SUPPLIER BELONGS TO CURRENT PHARMACY
            // -----------------------------------------------------

            var existingSupplier =
                SupplierStorage.GetById(
                    supplier.Id,
                    pharmacyId
                );

            if (existingSupplier == null)
            {
                TempData["ErrorMessage"] =
                    "Supplier not found or you do not have access to it.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // CLEAN INPUT
            // -----------------------------------------------------

            supplier.Name =
                supplier.Name?.Trim() ?? string.Empty;

            supplier.Phone =
                supplier.Phone?.Trim() ?? string.Empty;

            supplier.Email =
                supplier.Email?.Trim() ?? string.Empty;

            supplier.Address =
                supplier.Address?.Trim() ?? string.Empty;

            supplier.Company =
                supplier.Company?.Trim() ?? string.Empty;


            // -----------------------------------------------------
            // REQUIRED VALIDATION
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(supplier.Name))
            {
                ModelState.AddModelError(
                    "Name",
                    "Supplier name is required."
                );
            }

            if (string.IsNullOrWhiteSpace(supplier.Phone))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Phone number is required."
                );
            }


            // -----------------------------------------------------
            // DUPLICATE NAME CHECK
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(supplier.Name))
            {
                var duplicateSupplier =
                    SupplierStorage.GetByName(
                        supplier.Name,
                        pharmacyId
                    );

                if (duplicateSupplier != null &&
                    duplicateSupplier.Id != supplier.Id)
                {
                    ModelState.AddModelError(
                        "Name",
                        "Another supplier with this name already exists in your pharmacy."
                    );
                }
            }


            // -----------------------------------------------------
            // UPDATE
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                bool success =
                    SupplierStorage.Update(
                        supplier,
                        pharmacyId
                    );

                if (success)
                {
                    TempData["SuccessMessage"] =
                        "Supplier updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    "",
                    "Unable to update supplier. Please try again."
                );
            }

            return View(supplier);
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            int pharmacyId = CurrentPharmacyId;

            var supplier =
                SupplierStorage.GetById(
                    id,
                    pharmacyId
                );

            if (supplier == null)
            {
                TempData["ErrorMessage"] =
                    "Supplier not found or you do not have access to it.";

                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!PermissionChecker.HasPermission(User, "Stock Management"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            int pharmacyId = CurrentPharmacyId;


            // -----------------------------------------------------
            // VERIFY OWNERSHIP BEFORE DELETE
            // -----------------------------------------------------

            var supplier =
                SupplierStorage.GetById(
                    id,
                    pharmacyId
                );

            if (supplier == null)
            {
                TempData["ErrorMessage"] =
                    "Supplier not found or you do not have access to it.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // DELETE
            // -----------------------------------------------------

            bool success =
                SupplierStorage.Delete(
                    id,
                    pharmacyId
                );

            if (success)
            {
                TempData["SuccessMessage"] =
                    "Supplier deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Unable to delete supplier. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}