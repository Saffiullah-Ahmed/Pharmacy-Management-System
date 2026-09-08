using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class PurchaseController : PharmacyControllerBase
    {
        // =========================================================
        // INDEX
        // =========================================================

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

            var purchases =
                PurchaseStorage.Load(pharmacyId);

            return View(purchases);
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

            int pharmacyId = CurrentPharmacyId;

            ViewBag.Medicines =
                MedicineStorage.Load(pharmacyId);

            ViewBag.Suppliers =
                SupplierStorage.Load(pharmacyId);

            ViewBag.InvoiceNumber =
                PurchaseStorage.GenerateInvoiceNumber(
                    pharmacyId
                );

            return View(new Purchase
            {
                PurchaseDate = DateTime.Now,
                Items = new List<PurchaseItem>()
            });
        }


        // =========================================================
        // SEARCH MEDICINES
        // =========================================================
        //
        // IMPORTANT:
        // The Create.cshtml page calls this action when the user
        // searches for a medicine.
        //
        // Medicines are loaded using the CURRENT pharmacy ID.
        // Therefore Pharmacy A can never receive Pharmacy B medicines.
        //
        // =========================================================

        [HttpGet]
        public IActionResult SearchMedicines(string? term)
        {
            if (!PermissionChecker.HasPermission(
                    User,
                    "Stock Management"))
            {
                return Unauthorized();
            }

            var pharmacyCheck = RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return Unauthorized();
            }

            int pharmacyId = CurrentPharmacyId;

            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            term = term.Trim();

            // -----------------------------------------------------
            // IMPORTANT SECURITY RULE:
            //
            // MedicineStorage.Load(pharmacyId) only loads medicines
            // belonging to the logged-in pharmacy.
            //
            // We NEVER search all medicines from the database.
            // -----------------------------------------------------

            var medicines =
                MedicineStorage.Load(pharmacyId);

            var results =
                medicines
                    .Where(m =>
                        m.Name.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase
                        )
                        ||
                        (!string.IsNullOrWhiteSpace(m.Company)
                         &&
                         m.Company.Contains(
                             term,
                             StringComparison.OrdinalIgnoreCase
                         ))
                        ||
                        (!string.IsNullOrWhiteSpace(m.Category)
                         &&
                         m.Category.Contains(
                             term,
                             StringComparison.OrdinalIgnoreCase
                         ))
                        ||
                        (
                            m.Id.ToString()
                                .Contains(
                                    term,
                                    StringComparison.OrdinalIgnoreCase
                                )
                        )
                        ||
                        (
                            !string.IsNullOrWhiteSpace(
                                m.CompanyMedicineId
                            )
                            &&
                            m.CompanyMedicineId.Contains(
                                term,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    .Take(20)
                    .Select(m => new
                    {
                        id = m.Id,
                        name = m.Name,
                        company = m.Company,
                        category = m.Category,
                        companyMedicineId =
                            m.CompanyMedicineId,
                        price = m.Price,
                        quantity = m.Quantity
                    })
                    .ToList();

            return Json(results);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Purchase purchase)
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
            // BASIC VALIDATION
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    purchase.SupplierName))
            {
                ModelState.AddModelError(
                    "SupplierName",
                    "Please select a supplier."
                );
            }

            if (purchase.Items == null ||
                purchase.Items.Count == 0)
            {
                ModelState.AddModelError(
                    "Items",
                    "Please add at least one medicine."
                );
            }

            if (purchase.PurchaseDate == default)
            {
                purchase.PurchaseDate =
                    DateTime.Now;
            }


            // -----------------------------------------------------
            // VERIFY SUPPLIER BELONGS TO CURRENT PHARMACY
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    purchase.SupplierName))
            {
                var supplier =
                    SupplierStorage.GetByName(
                        purchase.SupplierName.Trim(),
                        pharmacyId
                    );

                if (supplier == null)
                {
                    ModelState.AddModelError(
                        "SupplierName",
                        "Selected supplier does not belong to your pharmacy."
                    );
                }
            }


            // -----------------------------------------------------
            // VALIDATE ITEMS
            // -----------------------------------------------------

            if (purchase.Items != null)
            {
                foreach (var item in purchase.Items)
                {
                    if (item.MedicineId <= 0)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "Please select a valid medicine."
                        );

                        continue;
                    }

                    if (item.Quantity <= 0)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "Medicine quantity must be greater than zero."
                        );
                    }

                    if (item.CostPrice < 0)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "Cost price cannot be negative."
                        );
                    }

                    // -------------------------------------------------
                    // SECURITY:
                    // Verify that the submitted medicine belongs
                    // to the current pharmacy.
                    // -------------------------------------------------

                    var medicine =
                        MedicineStorage.GetById(
                            item.MedicineId,
                            pharmacyId
                        );

                    if (medicine == null)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "One of the selected medicines does not belong to your pharmacy."
                        );
                    }
                    else
                    {
                        // Do not trust medicine name coming from browser.
                        item.MedicineName =
                            medicine.Name;
                    }
                }
            }


            // -----------------------------------------------------
            // SAVE PURCHASE
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(
                        purchase.InvoiceNumber))
                {
                    purchase.InvoiceNumber =
                        PurchaseStorage.GenerateInvoiceNumber(
                            pharmacyId
                        );
                }

                bool success =
                    PurchaseStorage.Add(
                        purchase,
                        pharmacyId
                    );

                if (success)
                {
                    TempData["SuccessMessage"] =
                        "Purchase added successfully and stock has been updated.";

                    return RedirectToAction(
                        nameof(Index)
                    );
                }

                ModelState.AddModelError(
                    "",
                    "Unable to save the purchase. Please try again."
                );
            }


            // -----------------------------------------------------
            // RELOAD DATA WHEN VALIDATION FAILS
            // -----------------------------------------------------

            ViewBag.Medicines =
                MedicineStorage.Load(pharmacyId);

            ViewBag.Suppliers =
                SupplierStorage.Load(pharmacyId);

            if (string.IsNullOrWhiteSpace(
                    purchase.InvoiceNumber))
            {
                ViewBag.InvoiceNumber =
                    PurchaseStorage.GenerateInvoiceNumber(
                        pharmacyId
                    );
            }
            else
            {
                ViewBag.InvoiceNumber =
                    purchase.InvoiceNumber;
            }

            return View(purchase);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public IActionResult Details(int id)
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

            var purchase =
                PurchaseStorage.GetById(
                    id,
                    pharmacyId
                );

            if (purchase == null)
            {
                TempData["ErrorMessage"] =
                    "Purchase not found.";

                return RedirectToAction(
                    nameof(Index)
                );
            }

            return View(purchase);
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

            var purchase =
                PurchaseStorage.GetById(
                    id,
                    pharmacyId
                );

            if (purchase == null)
            {
                TempData["ErrorMessage"] =
                    "Purchase not found.";

                return RedirectToAction(
                    nameof(Index)
                );
            }

            ViewBag.Medicines =
                MedicineStorage.Load(pharmacyId);

            ViewBag.Suppliers =
                SupplierStorage.Load(pharmacyId);

            return View(purchase);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Purchase purchase)
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
            // VERIFY PURCHASE BELONGS TO CURRENT PHARMACY
            // -----------------------------------------------------

            var existingPurchase =
                PurchaseStorage.GetById(
                    purchase.Id,
                    pharmacyId
                );

            if (existingPurchase == null)
            {
                TempData["ErrorMessage"] =
                    "Purchase not found or you do not have access to it.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // -----------------------------------------------------
            // BASIC VALIDATION
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    purchase.SupplierName))
            {
                ModelState.AddModelError(
                    "SupplierName",
                    "Please select a supplier."
                );
            }

            if (purchase.Items == null ||
                purchase.Items.Count == 0)
            {
                ModelState.AddModelError(
                    "Items",
                    "Please add at least one medicine."
                );
            }

            if (purchase.PurchaseDate == default)
            {
                purchase.PurchaseDate =
                    existingPurchase.PurchaseDate;
            }


            // -----------------------------------------------------
            // VERIFY SUPPLIER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    purchase.SupplierName))
            {
                var supplier =
                    SupplierStorage.GetByName(
                        purchase.SupplierName.Trim(),
                        pharmacyId
                    );

                if (supplier == null)
                {
                    ModelState.AddModelError(
                        "SupplierName",
                        "Selected supplier does not belong to your pharmacy."
                    );
                }
            }


            // -----------------------------------------------------
            // VALIDATE ITEMS
            // -----------------------------------------------------

            if (purchase.Items != null)
            {
                foreach (var item in purchase.Items)
                {
                    if (item.MedicineId <= 0)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "Please select a valid medicine."
                        );

                        continue;
                    }

                    if (item.Quantity <= 0)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "Medicine quantity must be greater than zero."
                        );
                    }

                    if (item.CostPrice < 0)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "Cost price cannot be negative."
                        );
                    }

                    var medicine =
                        MedicineStorage.GetById(
                            item.MedicineId,
                            pharmacyId
                        );

                    if (medicine == null)
                    {
                        ModelState.AddModelError(
                            "Items",
                            "One of the selected medicines does not belong to your pharmacy."
                        );
                    }
                    else
                    {
                        item.MedicineName =
                            medicine.Name;
                    }
                }
            }


            // -----------------------------------------------------
            // SAVE CHANGES
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                purchase.InvoiceNumber =
                    existingPurchase.InvoiceNumber;

                bool success =
                    PurchaseStorage.Update(
                        purchase,
                        pharmacyId
                    );

                if (success)
                {
                    TempData["SuccessMessage"] =
                        "Purchase updated successfully and stock has been adjusted.";

                    return RedirectToAction(
                        nameof(Details),
                        new
                        {
                            id = purchase.Id
                        }
                    );
                }

                ModelState.AddModelError(
                    "",
                    "Unable to update the purchase. Please check stock availability and try again."
                );
            }


            // -----------------------------------------------------
            // RELOAD DATA
            // -----------------------------------------------------

            ViewBag.Medicines =
                MedicineStorage.Load(pharmacyId);

            ViewBag.Suppliers =
                SupplierStorage.Load(pharmacyId);

            return View(purchase);
        }


        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
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


            // -----------------------------------------------------
            // VERIFY PURCHASE BELONGS TO CURRENT PHARMACY
            // -----------------------------------------------------

            var purchase =
                PurchaseStorage.GetById(
                    id,
                    pharmacyId
                );

            if (purchase == null)
            {
                TempData["ErrorMessage"] =
                    "Purchase not found or you do not have access to it.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // -----------------------------------------------------
            // DELETE
            // -----------------------------------------------------

            bool success =
                PurchaseStorage.Delete(
                    id,
                    pharmacyId
                );

            if (success)
            {
                TempData["SuccessMessage"] =
                    "Purchase deleted successfully and stock has been adjusted.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Unable to delete the purchase. Stock may not have enough quantity to reverse this purchase.";
            }

            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}