using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class CustomerCreditController : PharmacyControllerBase
    {
        // ============================================================
        // PENDING PAYMENTS
        // ============================================================

        [HttpGet]
        public IActionResult Index(string? search)
        {
            if (!HasSalesPermission())
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck =
                RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            search = search?.Trim();

            List<Customer> customers =
                CustomerCreditStorage
                    .GetCustomersWithPendingPayments(
                        CurrentPharmacyId,
                        search);

            ViewBag.PharmacyId =
                CurrentPharmacyId;

            ViewBag.Search =
                search ?? string.Empty;

            return View(customers);
        }


        // ============================================================
        // CUSTOMER DETAILS
        // ============================================================

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!HasSalesPermission())
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck =
                RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            if (id <= 0)
            {
                return NotFound();
            }


            // ========================================================
            // GET CUSTOMER
            // ========================================================

            Customer? customer =
                CustomerStorage.GetById(
                    id,
                    CurrentPharmacyId);

            if (customer == null)
            {
                return NotFound();
            }


            // ========================================================
            // GET CUSTOMER BALANCE
            // ========================================================

            decimal balance =
                CustomerCreditStorage
                    .GetCustomerBalance(
                        id,
                        CurrentPharmacyId);


            // ========================================================
            // GET PENDING SALES
            // ========================================================

            List<Sale> pendingSales =
                CustomerCreditStorage
                    .GetPendingSales(
                        id,
                        CurrentPharmacyId);


            // ========================================================
            // GET PAYMENT HISTORY
            // ========================================================

            List<CustomerPayment> payments =
                CustomerCreditStorage
                    .GetPaymentHistory(
                        id,
                        CurrentPharmacyId);


            ViewBag.Balance =
                balance;

            ViewBag.PendingSales =
                pendingSales;

            ViewBag.Payments =
                payments;

            ViewBag.PharmacyId =
                CurrentPharmacyId;

            return View(customer);
        }


        // ============================================================
        // RECEIVE PAYMENT - GET
        // ============================================================

        [HttpGet]
        public IActionResult ReceivePayment(
            int customerId,
            int saleId)
        {
            if (!HasSalesPermission())
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck =
                RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            if (customerId <= 0 ||
                saleId <= 0)
            {
                return BadRequest();
            }


            // ========================================================
            // GET CUSTOMER
            // ========================================================

            Customer? customer =
                CustomerStorage.GetById(
                    customerId,
                    CurrentPharmacyId);

            if (customer == null)
            {
                return NotFound();
            }


            // ========================================================
            // GET SALE
            // ========================================================

            Sale? sale =
                SaleStorage.GetById(
                    saleId,
                    CurrentPharmacyId);

            if (sale == null)
            {
                return NotFound();
            }


            // ========================================================
            // VERIFY SALE BELONGS TO CUSTOMER
            // ========================================================

            if (sale.CustomerId != customerId)
            {
                return BadRequest(
                    "This sale does not belong to this customer.");
            }


            // ========================================================
            // CHECK PENDING BALANCE
            // ========================================================

            if (sale.BalanceAmount <= 0)
            {
                return BadRequest(
                    "This sale has no pending balance.");
            }


            ViewBag.Customer =
                customer;

            ViewBag.Sale =
                sale;

            ViewBag.PharmacyId =
                CurrentPharmacyId;


            // Amount remains 0 internally,
            // but the view displays it as blank.

            return View(
                new CustomerPayment
                {
                    CustomerId = customerId,
                    SaleId = saleId
                });
        }


        // ============================================================
        // RECEIVE PAYMENT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReceivePayment(
            CustomerPayment payment)
        {
            if (!HasSalesPermission())
            {
                return Forbid();
            }

            IActionResult? pharmacyCheck =
                RequirePharmacy();

            if (pharmacyCheck != null)
            {
                return pharmacyCheck;
            }

            if (payment == null)
            {
                return BadRequest();
            }

            if (payment.CustomerId <= 0 ||
                payment.SaleId <= 0)
            {
                return BadRequest();
            }


            // ========================================================
            // GET CUSTOMER
            // ========================================================

            Customer? customer =
                CustomerStorage.GetById(
                    payment.CustomerId,
                    CurrentPharmacyId);


            // ========================================================
            // GET SALE
            // ========================================================

            Sale? sale =
                SaleStorage.GetById(
                    payment.SaleId,
                    CurrentPharmacyId);


            if (customer == null ||
                sale == null)
            {
                return NotFound();
            }


            // ========================================================
            // VERIFY SALE BELONGS TO CUSTOMER
            // ========================================================

            if (sale.CustomerId !=
                payment.CustomerId)
            {
                return BadRequest(
                    "This sale does not belong to this customer.");
            }


            // ========================================================
            // VALIDATE BALANCE
            // ========================================================

            if (sale.BalanceAmount <= 0)
            {
                ModelState.AddModelError(
                    "Amount",
                    "This sale has no pending balance.");
            }


            // ========================================================
            // VALIDATE PAYMENT AMOUNT
            // ========================================================

            if (payment.Amount <= 0)
            {
                ModelState.AddModelError(
                    "Amount",
                    "Payment amount must be greater than zero.");
            }


            // ========================================================
            // PAYMENT CANNOT EXCEED BALANCE
            // ========================================================

            if (payment.Amount >
                sale.BalanceAmount)
            {
                ModelState.AddModelError(
                    "Amount",
                    "Payment cannot be greater than the pending balance.");
            }


            // ========================================================
            // RETURN FORM WITH ERRORS
            // ========================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Customer =
                    customer;

                ViewBag.Sale =
                    sale;

                ViewBag.PharmacyId =
                    CurrentPharmacyId;

                return View(payment);
            }


            // ========================================================
            // SAVE PAYMENT
            // ========================================================

            bool success =
                CustomerCreditStorage.ReceivePayment(
                    payment.CustomerId,
                    payment.SaleId,
                    payment.Amount,
                    payment.Notes ?? string.Empty,
                    CurrentPharmacyId);

            if (!success)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to save the payment. " +
                    "The sale balance may have changed. " +
                    "Please refresh and try again.");

                ViewBag.Customer =
                    customer;

                ViewBag.Sale =
                    sale;

                ViewBag.PharmacyId =
                    CurrentPharmacyId;

                return View(payment);
            }


            // ========================================================
            // SUCCESS
            // ========================================================

            TempData["SuccessMessage"] =
                "Payment received successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = payment.CustomerId
                });
        }


        // ============================================================
        // PERMISSION HELPER
        // ============================================================

        private bool HasSalesPermission()
        {
            return PermissionChecker.HasPermission(
                User,
                "Sales Management");
        }
    }
}