using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class ReturnController : PharmacyControllerBase
    {
        // ============================================================
        // SALES PERMISSION
        // ============================================================

        private bool HasSalesPermission()
        {
            if (CurrentRole == "Admin")
                return true;

            return User.Claims.Any(c =>
                c.Type == "Permission" &&
                c.Value == "Sales Management");
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpGet]
        public IActionResult Create(int saleId = 0)
        {
            if (!HasSalesPermission())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }

            if (!HasValidPharmacy())
            {
                return RequirePharmacy()
                    ?? RedirectToAction(
                        "Login",
                        "Account");
            }

            int pharmacyId = CurrentPharmacyId;

            // --------------------------------------------------------
            // No sale selected yet
            // --------------------------------------------------------

            if (saleId <= 0)
                return View();


            // --------------------------------------------------------
            // Get sale
            // --------------------------------------------------------

            Sale? sale =
                ReturnStorage.GetSaleForReturn(
                    saleId,
                    pharmacyId);

            if (sale == null)
            {
                TempData["ErrorMessage"] =
                    "Sale not found or you do not have access to this sale.";

                return RedirectToAction(
                    "Index",
                    "Sale");
            }


            // --------------------------------------------------------
            // Get returnable items
            // --------------------------------------------------------

            sale.Items =
                ReturnStorage.GetSaleItems(
                    saleId,
                    pharmacyId);

            return View(sale);
        }


        // ============================================================
        // FIND SALE BY INVOICE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FindInvoice(
            string invoiceNumber)
        {
            if (!HasSalesPermission())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }

            if (!HasValidPharmacy())
            {
                return RequirePharmacy()
                    ?? RedirectToAction(
                        "Login",
                        "Account");
            }

            // --------------------------------------------------------
            // Validate invoice
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                TempData["ErrorMessage"] =
                    "Please enter an invoice number.";

                return RedirectToAction(
                    nameof(Create));
            }


            int pharmacyId = CurrentPharmacyId;


            // --------------------------------------------------------
            // Find sale
            // --------------------------------------------------------

            Sale? sale =
                ReturnStorage.GetSaleByInvoice(
                    invoiceNumber,
                    pharmacyId);

            if (sale == null)
            {
                TempData["ErrorMessage"] =
                    "Invoice not found.";

                return RedirectToAction(
                    nameof(Create));
            }


            // --------------------------------------------------------
            // Get returnable items
            // --------------------------------------------------------

            sale.Items =
                ReturnStorage.GetSaleItems(
                    sale.Id,
                    pharmacyId);

            return View(
                "Create",
                sale);
        }


        // ============================================================
        // PROCESS RETURN
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessReturn(
            int saleId,
            List<SaleReturnItem>? items,
            string? reason)
        {
            if (!HasSalesPermission())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }

            if (!HasValidPharmacy())
            {
                return RequirePharmacy()
                    ?? RedirectToAction(
                        "Login",
                        "Account");
            }

            int pharmacyId = CurrentPharmacyId;


            // --------------------------------------------------------
            // Validate sale
            // --------------------------------------------------------

            Sale? sale =
                ReturnStorage.GetSaleForReturn(
                    saleId,
                    pharmacyId);

            if (sale == null)
            {
                TempData["ErrorMessage"] =
                    "Sale not found or you do not have access to this sale.";

                return RedirectToAction(
                    "Index",
                    "Sale");
            }


            // --------------------------------------------------------
            // Validate posted items
            // --------------------------------------------------------

            if (items == null ||
                items.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select at least one medicine to return.";

                return RedirectToAction(
                    nameof(Create),
                    new { saleId });
            }


            // --------------------------------------------------------
            // Get remaining returnable items
            // --------------------------------------------------------

            List<SaleItem> saleItems =
                ReturnStorage.GetSaleItems(
                    saleId,
                    pharmacyId);

            if (saleItems.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "There are no medicines available to return for this sale.";

                return RedirectToAction(
                    nameof(Create),
                    new { saleId });
            }


            // --------------------------------------------------------
            // Create return
            // --------------------------------------------------------

            SaleReturn saleReturn =
                new SaleReturn
                {
                    SaleId =
                        saleId,

                    ReturnDate =
                        DateTime.Now,

                    Reason =
                        reason?.Trim()
                        ?? string.Empty,

                    TotalRefund =
                        0,

                    Items =
                        new List<SaleReturnItem>()
                };


            decimal totalRefund = 0;


            // --------------------------------------------------------
            // Validate each posted item
            // --------------------------------------------------------

            foreach (SaleReturnItem postedItem in items)
            {
                if (postedItem == null)
                    continue;

                if (postedItem.Quantity <= 0)
                    continue;


                // ----------------------------------------------------
                // Find matching sale item
                // ----------------------------------------------------

                SaleItem? saleItem =
                    saleItems.FirstOrDefault(
                        x =>
                            x.Id ==
                            postedItem.SaleItemId);

                if (saleItem == null)
                {
                    TempData["ErrorMessage"] =
                        "One or more selected sale items are invalid.";

                    return RedirectToAction(
                        nameof(Create),
                        new { saleId });
                }


                // ----------------------------------------------------
                // Validate medicine ID
                // ----------------------------------------------------

                if (!saleItem.MedicineId.HasValue)
                {
                    TempData["ErrorMessage"] =
                        $"Medicine information is missing for " +
                        $"{saleItem.MedicineName}.";

                    return RedirectToAction(
                        nameof(Create),
                        new { saleId });
                }


                int medicineId =
                    saleItem.MedicineId.Value;


                // ----------------------------------------------------
                // Validate quantity
                // ----------------------------------------------------

                if (!saleItem.Quantity.HasValue)
                {
                    TempData["ErrorMessage"] =
                        $"Quantity information is missing for " +
                        $"{saleItem.MedicineName}.";

                    return RedirectToAction(
                        nameof(Create),
                        new { saleId });
                }


                int availableQuantity =
                    saleItem.Quantity.Value;


                if (postedItem.Quantity >
                    availableQuantity)
                {
                    TempData["ErrorMessage"] =
                        $"Return quantity for " +
                        $"{saleItem.MedicineName} " +
                        $"cannot be greater than the remaining " +
                        $"returnable quantity.";

                    return RedirectToAction(
                        nameof(Create),
                        new { saleId });
                }


                // ----------------------------------------------------
                // Calculate refund
                // ----------------------------------------------------

                decimal itemRefund =
                    postedItem.Quantity *
                    saleItem.UnitPrice;

                totalRefund +=
                    itemRefund;


                // ----------------------------------------------------
                // Add validated return item
                // ----------------------------------------------------

                saleReturn.Items.Add(
                    new SaleReturnItem
                    {
                        SaleItemId =
                            saleItem.Id,

                        MedicineId =
                            medicineId,

                        MedicineName =
                            saleItem.MedicineName,

                        Quantity =
                            postedItem.Quantity,

                        UnitPrice =
                            saleItem.UnitPrice,

                        TotalRefund =
                            itemRefund
                    });
            }


            // --------------------------------------------------------
            // Validate final item list
            // --------------------------------------------------------

            if (saleReturn.Items.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "No valid medicine was selected for return.";

                return RedirectToAction(
                    nameof(Create),
                    new { saleId });
            }


            // --------------------------------------------------------
            // Set refund total
            // --------------------------------------------------------

            saleReturn.TotalRefund =
                totalRefund;


            // --------------------------------------------------------
            // Save return
            // --------------------------------------------------------

            try
            {
                ReturnStorage.Add(
                    saleReturn,
                    pharmacyId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    string.IsNullOrWhiteSpace(ex.Message)
                        ? "Unable to process the return."
                        : ex.Message;

                return RedirectToAction(
                    nameof(Create),
                    new { saleId });
            }


            // --------------------------------------------------------
            // Success
            // --------------------------------------------------------

            TempData["SuccessMessage"] =
                $"Return processed successfully. " +
                $"Refund: Rs. {totalRefund:N2}";

            return RedirectToAction(
                nameof(ReturnHistory));
        }


        // ============================================================
        // RETURN HISTORY
        // ============================================================

        [HttpGet]
        public IActionResult ReturnHistory()
        {
            if (!HasSalesPermission())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account");
            }

            if (!HasValidPharmacy())
            {
                return RequirePharmacy()
                    ?? RedirectToAction(
                        "Login",
                        "Account");
            }

            int pharmacyId = CurrentPharmacyId;


            List<SaleReturn> returns =
                ReturnStorage.Load(
                    pharmacyId);

            return View(returns);
        }
    }
}