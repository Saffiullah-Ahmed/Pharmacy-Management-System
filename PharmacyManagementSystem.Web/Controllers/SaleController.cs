using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;
using System.Text;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class SaleController : PharmacyControllerBase
    {
        private bool HasSalesPermission()
        {
            if (User.IsInRole("Admin"))
                return true;

            var permissions =
                User.Claims
                    .Where(c =>
                        c.Type.Equals("Permission", StringComparison.OrdinalIgnoreCase)
                        || c.Type.Equals("Permissions", StringComparison.OrdinalIgnoreCase)
                        || c.Type.Equals("permission", StringComparison.OrdinalIgnoreCase)
                        || c.Type.Equals("permissions", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(c =>
                        c.Value.Split(
                            new[] { ',', ';', '|' },
                            StringSplitOptions.RemoveEmptyEntries |
                            StringSplitOptions.TrimEntries));

            return permissions.Any(p =>
                string.Equals(
                    p,
                    "Sales Management",
                    StringComparison.OrdinalIgnoreCase));
        }


        // ==========================================================
        // SALES HISTORY
        // ==========================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!HasSalesPermission())
                return RedirectToAction("AccessDenied", "Account");

            int pharmacyId = GetPharmacyId();

            if (pharmacyId <= 0)
                return RedirectToAction("Login", "Account");

            var sales = SaleStorage.Load(pharmacyId);

            var returns = ReturnStorage.Load(pharmacyId);

            var refundBySale =
                returns
                    .GroupBy(x => x.SaleId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Sum(r => r.TotalRefund));

            decimal totalRefund =
                returns.Sum(x => x.TotalRefund);

            DateTime today = DateTime.Today;

            var todaySales =
                sales
                    .Where(x => x.SaleDate.Date == today)
                    .ToList();

            var todayReturns =
                returns
                    .Where(x => x.ReturnDate.Date == today)
                    .ToList();

            decimal todayRefund =
                todayReturns.Sum(x => x.TotalRefund);

            decimal todayGrossSales =
                Math.Max(
                    0,
                    todaySales.Sum(x => x.TotalAmount)
                    - todayRefund);

            decimal todayNetSales =
                Math.Max(
                    0,
                    todaySales.Sum(x => x.NetAmount)
                    - todayRefund);

            ViewBag.Returns = returns;
            ViewBag.RefundBySale = refundBySale;
            ViewBag.TotalRefund = totalRefund;
            ViewBag.TodaySalesCount = todaySales.Count;
            ViewBag.TodayGrossSales = todayGrossSales;
            ViewBag.TodayNetSales = todayNetSales;
            ViewBag.TodayRefund = todayRefund;

            return View(sales);
        }


        // ==========================================================
        // CREATE SALE - GET
        // ==========================================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!HasSalesPermission())
                return RedirectToAction("AccessDenied", "Account");

            int pharmacyId = GetPharmacyId();

            if (pharmacyId <= 0)
                return RedirectToAction("Login", "Account");

            ViewBag.Medicines =
                MedicineStorage.Load(pharmacyId);

            ViewBag.Customers =
                CustomerStorage.Load(pharmacyId);

            var sale = new Sale
            {
                GenerateBill = true,
                AmountReceived = 0,
                DiscountAmount = 0,
                CustomerId = null
            };

            return View(sale);
        }


        // ==========================================================
        // CREATE SALE - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sale sale)
        {
            if (!HasSalesPermission())
                return RedirectToAction("AccessDenied", "Account");

            int pharmacyId = GetPharmacyId();

            if (pharmacyId <= 0)
                return RedirectToAction("Login", "Account");

            // Always load only this pharmacy's data.
            ViewBag.Medicines =
                MedicineStorage.Load(pharmacyId);

            ViewBag.Customers =
                CustomerStorage.Load(pharmacyId);

            sale.Items ??= new List<SaleItem>();


            // ------------------------------------------------------
            // Invoice number is generated by the server.
            // ------------------------------------------------------

            ModelState.Remove(nameof(Sale.InvoiceNumber));


            // ------------------------------------------------------
            // At least one item is required.
            // ------------------------------------------------------

            if (!sale.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one medicine.");
            }


            // ------------------------------------------------------
            // Validate every sale item.
            // ------------------------------------------------------

            foreach (var item in sale.Items)
            {
                if (!item.MedicineId.HasValue ||
                    item.MedicineId.Value <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Please select a medicine for every sale item.");
                }

                if (!item.Quantity.HasValue ||
                    item.Quantity.Value <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Medicine quantity must be greater than zero.");
                }
            }


            // ------------------------------------------------------
            // Validate customer.
            // ------------------------------------------------------

            if (sale.CustomerId.HasValue)
            {
                var customer =
                    CustomerStorage.GetById(
                        sale.CustomerId.Value,
                        pharmacyId);

                if (customer == null)
                {
                    ModelState.AddModelError(
                        nameof(Sale.CustomerId),
                        "Invalid customer.");
                }
            }


            // ------------------------------------------------------
            // Clean discount.
            // ------------------------------------------------------

            if (sale.DiscountAmount < 0)
                sale.DiscountAmount = 0;


            // ------------------------------------------------------
            // Calculate totals only from valid quantities.
            // Price is NOT trusted from the browser.
            //
            // The database will calculate the actual prices again
            // inside SaleStorage.Add().
            // ------------------------------------------------------

            decimal estimatedTotal = 0;

            foreach (var item in sale.Items)
            {
                if (item.MedicineId.HasValue &&
                    item.Quantity.HasValue)
                {
                    var medicine =
                        MedicineStorage.GetById(
                            item.MedicineId.Value,
                            pharmacyId);

                    if (medicine == null)
                    {
                        ModelState.AddModelError(
                            "",
                            "One of the selected medicines is invalid.");
                        continue;
                    }

                    estimatedTotal +=
                        item.Quantity.Value * medicine.Price;
                }
            }


            sale.TotalAmount = estimatedTotal;


            // Discount cannot exceed total.
            if (sale.DiscountAmount > sale.TotalAmount)
                sale.DiscountAmount = sale.TotalAmount;


            sale.NetAmount =
                sale.TotalAmount -
                sale.DiscountAmount;


            // ------------------------------------------------------
            // Payment calculation.
            // ------------------------------------------------------

            if (sale.AmountReceived < 0)
                sale.AmountReceived = 0;


            if (sale.AmountReceived >= sale.NetAmount)
            {
                sale.ChangeAmount =
                    sale.AmountReceived -
                    sale.NetAmount;

                sale.BalanceAmount = 0;
            }
            else
            {
                sale.ChangeAmount = 0;

                sale.BalanceAmount =
                    sale.NetAmount -
                    sale.AmountReceived;
            }


            // ------------------------------------------------------
            // Customer required for outstanding balance.
            // ------------------------------------------------------

            if (sale.BalanceAmount > 0 &&
                !sale.CustomerId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(Sale.CustomerId),
                    "A customer is required when there is an outstanding balance.");
            }


            // ------------------------------------------------------
            // Stop if validation failed.
            // ------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(sale);
            }


            // ------------------------------------------------------
            // Server-controlled sale information.
            // ------------------------------------------------------

            sale.PharmacyId = pharmacyId;

            sale.SaleDate = DateTime.Now;

            sale.InvoiceNumber =
                SaleStorage.GenerateInvoiceNumber(
                    pharmacyId);


            try
            {
                bool saved =
                    SaleStorage.Add(
                        sale,
                        pharmacyId);

                if (!saved)
                {
                    ModelState.AddModelError(
                        "",
                        "The sale could not be completed. Please check medicine stock and try again.");

                    return View(sale);
                }


                TempData["SuccessMessage"] =
                    $"Sale {sale.InvoiceNumber} completed successfully.";


                return RedirectToAction(
                    nameof(Bill),
                    new { id = sale.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(sale);
            }
        }


        // ==========================================================
        // BILL
        // ==========================================================

        [HttpGet]
        public IActionResult Bill(int id)
        {
            if (!HasSalesPermission())
                return RedirectToAction(
                    "AccessDenied",
                    "Account");

            int pharmacyId = GetPharmacyId();

            if (pharmacyId <= 0)
                return RedirectToAction(
                    "Login",
                    "Account");

            var sale =
                SaleStorage.GetById(
                    id,
                    pharmacyId);

            if (sale == null)
                return NotFound();

            return View(sale);
        }


        // ==========================================================
        // EXPORT SALES CSV
        // ==========================================================

        [HttpGet]
        public IActionResult ExportCsv()
        {
            if (!HasSalesPermission())
                return RedirectToAction(
                    "AccessDenied",
                    "Account");

            int pharmacyId = GetPharmacyId();

            if (pharmacyId <= 0)
                return RedirectToAction(
                    "Login",
                    "Account");

            var sales =
                SaleStorage.Load(
                    pharmacyId);

            var returns =
                ReturnStorage.Load(
                    pharmacyId);

            var refundBySale =
                returns
                    .GroupBy(x => x.SaleId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Sum(r => r.TotalRefund));


            var csv =
                new StringBuilder();


            csv.AppendLine(
                "Invoice Number,Sale Date,Original Gross Sales,Discount,Original Net Amount,Refund,Actual Gross Sales,Actual Net Sales,Amount Received,Change,Balance");


            foreach (var sale in
                sales.OrderByDescending(
                    x => x.SaleDate))
            {
                decimal refund =
                    refundBySale.TryGetValue(
                        sale.Id,
                        out var refundValue)
                        ? refundValue
                        : 0;


                decimal actualGrossSales =
                    Math.Max(
                        0,
                        sale.TotalAmount -
                        refund);


                decimal actualNetSales =
                    Math.Max(
                        0,
                        sale.NetAmount -
                        refund);


                csv.AppendLine(
                    $"{CsvEscape(sale.InvoiceNumber)}," +
                    $"{CsvEscape(sale.SaleDate.ToString("dd/MM/yyyy HH:mm"))}," +
                    $"{sale.TotalAmount:0.00}," +
                    $"{sale.DiscountAmount:0.00}," +
                    $"{sale.NetAmount:0.00}," +
                    $"{refund:0.00}," +
                    $"{actualGrossSales:0.00}," +
                    $"{actualNetSales:0.00}," +
                    $"{sale.AmountReceived:0.00}," +
                    $"{sale.ChangeAmount:0.00}," +
                    $"{sale.BalanceAmount:0.00}");
            }


            byte[] bytes =
                Encoding.UTF8.GetPreamble()
                .Concat(
                    Encoding.UTF8.GetBytes(
                        csv.ToString()))
                .ToArray();


            return File(
                bytes,
                "text/csv",
                $"Sales-History-{DateTime.Now:yyyy-MM-dd}.csv");
        }


        // ==========================================================
        // CSV ESCAPE
        // ==========================================================

        private static string CsvEscape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(',') ||
                value.Contains('"') ||
                value.Contains('\n') ||
                value.Contains('\r'))
            {
                return "\"" +
                       value.Replace("\"", "\"\"") +
                       "\"";
            }

            return value;
        }


        // ==========================================================
        // GET PHARMACY ID
        // ==========================================================

        private int GetPharmacyId()
        {
            var claim =
                User.FindFirst("PharmacyId")?.Value;

            if (int.TryParse(
                claim,
                out int pharmacyId))
            {
                return pharmacyId;
            }

            return 0;
        }
    }
}