using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class CustomerController : PharmacyControllerBase
    {
        // ============================================================
        // CUSTOMER LIST
        // ============================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            List<Customer> customers =
                CustomerStorage.Load(
                    CurrentPharmacyId);

            return View(customers);
        }


        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            return View();
        }


        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            bool success =
                CustomerStorage.Add(
                    customer,
                    CurrentPharmacyId);

            if (!success)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to add customer.");

                return View(customer);
            }

            TempData["SuccessMessage"] =
                "Customer added successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            Customer? customer =
                CustomerStorage.GetById(
                    id,
                    CurrentPharmacyId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Customer customer)
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            bool success =
                CustomerStorage.Update(
                    customer,
                    CurrentPharmacyId);

            if (!success)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update customer.");

                return View(customer);
            }

            TempData["SuccessMessage"] =
                "Customer updated successfully.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // DELETE - GET
        // ============================================================

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            Customer? customer =
                CustomerStorage.GetById(
                    id,
                    CurrentPharmacyId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }


        // ============================================================
        // DELETE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!PermissionChecker.HasPermission(
                User,
                "Sales Management"))
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            if (id <= 0)
            {
                TempData["ErrorMessage"] =
                    "Invalid customer.";

                return RedirectToAction(
                    nameof(Index));
            }

            bool success =
                CustomerStorage.Delete(
                    id,
                    CurrentPharmacyId);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Customer could not be deleted. " +
                    "The customer may have sales or payment history.";

                return RedirectToAction(
                    nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Customer deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }
    }
}