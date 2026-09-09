using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Models;
using PharmacyManagementSystem.Web.Database;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class ReturnStorage
    {
        // ============================================================
        // LOAD RETURN HISTORY
        // ============================================================

        public static List<SaleReturn> Load(
            int pharmacyId)
        {
            var returns =
                new List<SaleReturn>();

            if (pharmacyId <= 0)
                return returns;


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT
                    sr.id,
                    sr.sale_id,
                    sr.return_date,
                    sr.total_refund,
                    sr.reason
                FROM sale_returns sr
                INNER JOIN sales s
                    ON s.id = sr.sale_id
                WHERE s.pharmacy_id = @pharmacy_id
                ORDER BY sr.return_date DESC,
                         sr.id DESC;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            using var reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                var saleReturn =
                    new SaleReturn
                    {
                        Id =
                            Convert.ToInt32(
                                reader["id"]),

                        SaleId =
                            Convert.ToInt32(
                                reader["sale_id"]),

                        ReturnDate =
                            Convert.ToDateTime(
                                reader["return_date"]),

                        TotalRefund =
                            Convert.ToDecimal(
                                reader["total_refund"]),

                        Reason =
                            reader["reason"] ==
                            DBNull.Value
                                ? string.Empty
                                : reader["reason"]
                                    .ToString()
                                    ?? string.Empty
                    };


                returns.Add(
                    saleReturn);
            }


            reader.Close();


            foreach (var saleReturn in returns)
            {
                saleReturn.Items =
                    GetReturnItems(
                        saleReturn.Id,
                        pharmacyId);
            }


            return returns;
        }


        // ============================================================
        // GET RETURN ITEMS
        // ============================================================

        private static List<SaleReturnItem>
            GetReturnItems(
                int returnId,
                int pharmacyId)
        {
            var items =
                new List<SaleReturnItem>();


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT
                    sri.id,
                    sri.return_id,
                    sri.sale_item_id,
                    sri.medicine_id,
                    sri.quantity,
                    sri.unit_price,
                    sri.total_refund,
                    m.name AS medicine_name
                FROM sale_return_items sri
                INNER JOIN sale_returns sr
                    ON sr.id = sri.return_id
                INNER JOIN sales s
                    ON s.id = sr.sale_id
                INNER JOIN medicines m
                    ON m.id = sri.medicine_id
                WHERE sri.return_id = @return_id
                  AND s.pharmacy_id = @pharmacy_id
                  AND m.pharmacy_id = @pharmacy_id
                ORDER BY sri.id;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@return_id",
                returnId);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            using var reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                items.Add(
                    new SaleReturnItem
                    {
                        Id =
                            Convert.ToInt32(
                                reader["id"]),

                        ReturnId =
                            Convert.ToInt32(
                                reader["return_id"]),

                        SaleItemId =
                            Convert.ToInt32(
                                reader["sale_item_id"]),

                        MedicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]),

                        Quantity =
                            Convert.ToInt32(
                                reader["quantity"]),

                        UnitPrice =
                            Convert.ToDecimal(
                                reader["unit_price"]),

                        TotalRefund =
                            Convert.ToDecimal(
                                reader["total_refund"]),

                        MedicineName =
                            reader["medicine_name"]?
                                .ToString()
                            ?? string.Empty
                    });
            }


            return items;
        }


        // ============================================================
        // GET SALE FOR RETURN
        // ============================================================

        public static Sale? GetSaleForReturn(
            int saleId,
            int pharmacyId)
        {
            if (saleId <= 0 ||
                pharmacyId <= 0)
            {
                return null;
            }


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    sale_date,
                    customer_id,
                    total_amount,
                    discount_amount,
                    net_amount,
                    amount_received,
                    change_amount,
                    balance_amount,
                    generate_bill
                FROM sales
                WHERE id = @sale_id
                  AND pharmacy_id = @pharmacy_id
                LIMIT 1;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@sale_id",
                saleId);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            using var reader =
                command.ExecuteReader();


            if (!reader.Read())
                return null;


            return MapSale(
                reader);
        }


        // ============================================================
        // GET SALE BY INVOICE
        // ============================================================

        public static Sale? GetSaleByInvoice(
            string invoiceInput,
            int pharmacyId)
        {
            if (string.IsNullOrWhiteSpace(
                    invoiceInput) ||
                pharmacyId <= 0)
            {
                return null;
            }


            string invoice =
                invoiceInput.Trim();


            if (invoice.StartsWith(
                    "INV-",
                    StringComparison.OrdinalIgnoreCase))
            {
                invoice =
                    invoice.Substring(4);
            }


            if (int.TryParse(
                invoice,
                out int number))
            {
                invoice =
                    $"INV-{number:D5}";
            }
            else
            {
                invoice =
                    invoiceInput
                        .Trim()
                        .ToUpperInvariant();
            }


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    sale_date,
                    customer_id,
                    total_amount,
                    discount_amount,
                    net_amount,
                    amount_received,
                    change_amount,
                    balance_amount,
                    generate_bill
                FROM sales
                WHERE invoice_no = @invoice_no
                  AND pharmacy_id = @pharmacy_id
                LIMIT 1;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@invoice_no",
                invoice);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            using var reader =
                command.ExecuteReader();


            if (!reader.Read())
                return null;


            return MapSale(
                reader);
        }


        // ============================================================
        // GET SALE ITEMS THAT CAN STILL BE RETURNED
        // ============================================================

        public static List<SaleItem>
            GetSaleItems(
                int saleId,
                int pharmacyId)
        {
            var items =
                new List<SaleItem>();


            if (saleId <= 0 ||
                pharmacyId <= 0)
            {
                return items;
            }


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT
                    si.id,
                    si.sale_id,
                    si.medicine_id,
                    m.name AS medicine_name,
                    si.quantity,
                    si.price,
                    si.total,

                    COALESCE(
                        (
                            SELECT SUM(sri.quantity)
                            FROM sale_return_items sri
                            INNER JOIN sale_returns sr
                                ON sr.id = sri.return_id
                            INNER JOIN sales s2
                                ON s2.id = sr.sale_id
                            WHERE sri.sale_item_id = si.id
                              AND s2.pharmacy_id = @pharmacy_id
                        ),
                        0
                    ) AS returned_quantity

                FROM sale_items si

                INNER JOIN sales s
                    ON s.id = si.sale_id

                INNER JOIN medicines m
                    ON m.id = si.medicine_id

                WHERE si.sale_id = @sale_id
                  AND s.pharmacy_id = @pharmacy_id
                  AND m.pharmacy_id = @pharmacy_id

                ORDER BY si.id;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@sale_id",
                saleId);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            using var reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                int soldQuantity =
                    Convert.ToInt32(
                        reader["quantity"]);


                int returnedQuantity =
                    Convert.ToInt32(
                        reader["returned_quantity"]);


                int remaining =
                    soldQuantity -
                    returnedQuantity;


                if (remaining <= 0)
                    continue;


                decimal price =
                    Convert.ToDecimal(
                        reader["price"]);


                items.Add(
                    new SaleItem
                    {
                        Id =
                            Convert.ToInt32(
                                reader["id"]),

                        SaleId =
                            Convert.ToInt32(
                                reader["sale_id"]),

                        MedicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]),

                        MedicineName =
                            reader["medicine_name"]?
                                .ToString()
                            ?? string.Empty,

                        Quantity =
                            remaining,

                        UnitPrice =
                            price,

                        TotalPrice =
                            remaining *
                            price
                    });
            }


            return items;
        }


        // ============================================================
        // GET SOLD QUANTITY
        // ============================================================

        public static int GetSoldQuantity(
            int saleItemId,
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT si.quantity
                FROM sale_items si
                INNER JOIN sales s
                    ON s.id = si.sale_id
                WHERE si.id = @sale_item_id
                  AND s.pharmacy_id = @pharmacy_id
                LIMIT 1;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@sale_item_id",
                saleItemId);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                return 0;
            }


            return Convert.ToInt32(
                result);
        }


        // ============================================================
        // GET RETURNED QUANTITY
        // ============================================================

        public static int GetReturnedQuantity(
            int saleItemId,
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT COALESCE(
                    SUM(sri.quantity),
                    0
                )
                FROM sale_return_items sri
                INNER JOIN sale_returns sr
                    ON sr.id = sri.return_id
                INNER JOIN sales s
                    ON s.id = sr.sale_id
                WHERE sri.sale_item_id = @sale_item_id
                  AND s.pharmacy_id = @pharmacy_id;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@sale_item_id",
                saleItemId);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                return 0;
            }


            return Convert.ToInt32(
                result);
        }


        // ============================================================
        // GET REMAINING RETURNABLE QUANTITY
        // ============================================================

        public static int
            GetRemainingReturnableQuantity(
                int saleItemId,
                int pharmacyId)
        {
            int sold =
                GetSoldQuantity(
                    saleItemId,
                    pharmacyId);


            int returned =
                GetReturnedQuantity(
                    saleItemId,
                    pharmacyId);


            return Math.Max(
                0,
                sold - returned);
        }


        // ============================================================
        // FIND SALE ITEM
        // ============================================================

        public static SaleItem? FindSaleItem(
            int saleItemId,
            int pharmacyId)
        {
            if (saleItemId <= 0 ||
                pharmacyId <= 0)
            {
                return null;
            }


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            const string query = @"
                SELECT
                    si.id,
                    si.sale_id,
                    si.medicine_id,
                    si.quantity,
                    si.price,
                    si.total,
                    m.name AS medicine_name
                FROM sale_items si
                INNER JOIN sales s
                    ON s.id = si.sale_id
                INNER JOIN medicines m
                    ON m.id = si.medicine_id
                WHERE si.id = @sale_item_id
                  AND s.pharmacy_id = @pharmacy_id
                  AND m.pharmacy_id = @pharmacy_id
                LIMIT 1;";


            using var command =
                new MySqlCommand(
                    query,
                    connection);


            command.Parameters.AddWithValue(
                "@sale_item_id",
                saleItemId);


            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);


            using var reader =
                command.ExecuteReader();


            if (!reader.Read())
                return null;


            return new SaleItem
            {
                Id =
                    Convert.ToInt32(
                        reader["id"]),

                SaleId =
                    Convert.ToInt32(
                        reader["sale_id"]),

                MedicineId =
                    Convert.ToInt32(
                        reader["medicine_id"]),

                Quantity =
                    Convert.ToInt32(
                        reader["quantity"]),

                UnitPrice =
                    Convert.ToDecimal(
                        reader["price"]),

                TotalPrice =
                    Convert.ToDecimal(
                        reader["total"]),

                MedicineName =
                    reader["medicine_name"]?
                        .ToString()
                    ?? string.Empty
            };
        }


        // ============================================================
        // ADD RETURN
        // ============================================================

        public static void Add(
            SaleReturn saleReturn,
            int pharmacyId)
        {
            if (saleReturn == null)
            {
                throw new ArgumentNullException(
                    nameof(saleReturn));
            }


            if (pharmacyId <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid pharmacy.");
            }


            if (saleReturn.SaleId <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid sale.");
            }


            if (saleReturn.Items == null ||
                !saleReturn.Items.Any())
            {
                throw new InvalidOperationException(
                    "No medicine selected for return.");
            }


            // --------------------------------------------------------
            // IMPORTANT:
            // Combine duplicate sale item IDs.
            // --------------------------------------------------------

            var groupedItems =
                saleReturn.Items
                    .Where(x => x.Quantity > 0)
                    .GroupBy(x => x.SaleItemId)
                    .Select(g =>
                        new SaleReturnItem
                        {
                            SaleItemId =
                                g.Key,

                            Quantity =
                                g.Sum(
                                    x => x.Quantity)
                        })
                    .ToList();


            if (!groupedItems.Any())
            {
                throw new InvalidOperationException(
                    "No valid medicine selected for return.");
            }


            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            using var transaction =
                connection.BeginTransaction();


            try
            {
                // ====================================================
                // VERIFY SALE BELONGS TO PHARMACY
                // ====================================================

                const string saleQuery = @"
                    SELECT id
                    FROM sales
                    WHERE id = @sale_id
                      AND pharmacy_id = @pharmacy_id
                    LIMIT 1;";


                using (var saleCommand =
                    new MySqlCommand(
                        saleQuery,
                        connection,
                        transaction))
                {
                    saleCommand.Parameters.AddWithValue(
                        "@sale_id",
                        saleReturn.SaleId);

                    saleCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId);


                    object? saleResult =
                        saleCommand.ExecuteScalar();


                    if (saleResult == null)
                    {
                        throw new InvalidOperationException(
                            "Sale not found or does not belong to this pharmacy.");
                    }
                }


                // ====================================================
                // PREPARE RETURN
                // ====================================================

                decimal totalRefund = 0;


                var validatedItems =
                    new List<SaleReturnItem>();


                // ====================================================
                // VALIDATE EACH ITEM
                // ====================================================

                foreach (var returnItem in groupedItems)
                {
                    if (returnItem.Quantity <= 0)
                    {
                        throw new InvalidOperationException(
                            "Return quantity must be greater than zero.");
                    }


                    // ------------------------------------------------
                    // LOCK ORIGINAL SALE ITEM
                    // ------------------------------------------------

                    const string itemQuery = @"
                        SELECT
                            si.sale_id,
                            si.medicine_id,
                            si.quantity,
                            si.price,
                            m.name AS medicine_name
                        FROM sale_items si
                        INNER JOIN sales s
                            ON s.id = si.sale_id
                        INNER JOIN medicines m
                            ON m.id = si.medicine_id
                        WHERE si.id = @sale_item_id
                          AND si.sale_id = @sale_id
                          AND s.pharmacy_id = @pharmacy_id
                          AND m.pharmacy_id = @pharmacy_id
                        FOR UPDATE;";


                    int soldQuantity;
                    int medicineId;
                    decimal unitPrice;
                    string medicineName;


                    using (var itemCommand =
                        new MySqlCommand(
                            itemQuery,
                            connection,
                            transaction))
                    {
                        itemCommand.Parameters.AddWithValue(
                            "@sale_item_id",
                            returnItem.SaleItemId);

                        itemCommand.Parameters.AddWithValue(
                            "@sale_id",
                            saleReturn.SaleId);

                        itemCommand.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);


                        using var reader =
                            itemCommand.ExecuteReader();


                        if (!reader.Read())
                        {
                            throw new InvalidOperationException(
                                "Sale item not found or does not belong to this pharmacy.");
                        }


                        soldQuantity =
                            Convert.ToInt32(
                                reader["quantity"]);


                        medicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]);


                        unitPrice =
                            Convert.ToDecimal(
                                reader["price"]);


                        medicineName =
                            reader["medicine_name"]?
                                .ToString()
                            ?? string.Empty;
                    }


                    // ------------------------------------------------
                    // GET PREVIOUS RETURNS
                    // ------------------------------------------------

                    int alreadyReturned;


                    const string returnedQuery = @"
                        SELECT COALESCE(
                            SUM(sri.quantity),
                            0
                        )
                        FROM sale_return_items sri
                        INNER JOIN sale_returns sr
                            ON sr.id = sri.return_id
                        INNER JOIN sales s
                            ON s.id = sr.sale_id
                        WHERE sri.sale_item_id = @sale_item_id
                          AND s.pharmacy_id = @pharmacy_id;";


                    using (var returnedCommand =
                        new MySqlCommand(
                            returnedQuery,
                            connection,
                            transaction))
                    {
                        returnedCommand.Parameters.AddWithValue(
                            "@sale_item_id",
                            returnItem.SaleItemId);

                        returnedCommand.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);


                        alreadyReturned =
                            Convert.ToInt32(
                                returnedCommand.ExecuteScalar()
                                ?? 0);
                    }


                    int remaining =
                        soldQuantity -
                        alreadyReturned;


                    if (remaining <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Medicine '{medicineName}' has already been fully returned.");
                    }


                    if (returnItem.Quantity >
                        remaining)
                    {
                        throw new InvalidOperationException(
                            $"Only {remaining} unit(s) of '{medicineName}' can be returned.");
                    }


                    // ------------------------------------------------
                    // AUTHORITATIVE REFUND
                    // ------------------------------------------------

                    decimal itemRefund =
                        returnItem.Quantity *
                        unitPrice;


                    totalRefund +=
                        itemRefund;


                    validatedItems.Add(
                        new SaleReturnItem
                        {
                            SaleItemId =
                                returnItem.SaleItemId,

                            MedicineId =
                                medicineId,

                            Quantity =
                                returnItem.Quantity,

                            UnitPrice =
                                unitPrice,

                            TotalRefund =
                                itemRefund,

                            MedicineName =
                                medicineName
                        });
                }


                // ====================================================
                // INSERT RETURN HEADER
                // ====================================================

                const string returnQuery = @"
                    INSERT INTO sale_returns
                    (
                        sale_id,
                        return_date,
                        total_refund,
                        reason
                    )
                    VALUES
                    (
                        @sale_id,
                        @return_date,
                        @total_refund,
                        @reason
                    );";


                int returnId;


                using (var returnCommand =
                    new MySqlCommand(
                        returnQuery,
                        connection,
                        transaction))
                {
                    returnCommand.Parameters.AddWithValue(
                        "@sale_id",
                        saleReturn.SaleId);


                    returnCommand.Parameters.AddWithValue(
                        "@return_date",
                        saleReturn.ReturnDate);


                    returnCommand.Parameters.AddWithValue(
                        "@total_refund",
                        totalRefund);


                    returnCommand.Parameters.AddWithValue(
                        "@reason",
                        string.IsNullOrWhiteSpace(
                            saleReturn.Reason)
                            ? DBNull.Value
                            : saleReturn.Reason.Trim());


                    returnCommand.ExecuteNonQuery();


                    using var idCommand =
                        new MySqlCommand(
                            "SELECT LAST_INSERT_ID();",
                            connection,
                            transaction);


                    returnId =
                        Convert.ToInt32(
                            idCommand.ExecuteScalar());
                }


                // ====================================================
                // INSERT RETURN ITEMS + RESTORE STOCK
                // ====================================================

                foreach (var item in validatedItems)
                {
                    const string insertItemQuery = @"
                        INSERT INTO sale_return_items
                        (
                            return_id,
                            sale_item_id,
                            medicine_id,
                            quantity,
                            unit_price,
                            total_refund
                        )
                        VALUES
                        (
                            @return_id,
                            @sale_item_id,
                            @medicine_id,
                            @quantity,
                            @unit_price,
                            @total_refund
                        );";


                    using (var itemCommand =
                        new MySqlCommand(
                            insertItemQuery,
                            connection,
                            transaction))
                    {
                        itemCommand.Parameters.AddWithValue(
                            "@return_id",
                            returnId);


                        itemCommand.Parameters.AddWithValue(
                            "@sale_item_id",
                            item.SaleItemId);


                        itemCommand.Parameters.AddWithValue(
                            "@medicine_id",
                            item.MedicineId);


                        itemCommand.Parameters.AddWithValue(
                            "@quantity",
                            item.Quantity);


                        itemCommand.Parameters.AddWithValue(
                            "@unit_price",
                            item.UnitPrice);


                        itemCommand.Parameters.AddWithValue(
                            "@total_refund",
                            item.TotalRefund);


                        itemCommand.ExecuteNonQuery();
                    }


                    // ------------------------------------------------
                    // RESTORE STOCK
                    // ------------------------------------------------

                    const string stockQuery = @"
                        UPDATE medicines
                        SET quantity =
                            quantity + @quantity
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id;";


                    using var stockCommand =
                        new MySqlCommand(
                            stockQuery,
                            connection,
                            transaction);


                    stockCommand.Parameters.AddWithValue(
                        "@quantity",
                        item.Quantity);


                    stockCommand.Parameters.AddWithValue(
                        "@medicine_id",
                        item.MedicineId);


                    stockCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId);


                    int affected =
                        stockCommand.ExecuteNonQuery();


                    if (affected != 1)
                    {
                        throw new InvalidOperationException(
                            "Medicine stock could not be restored.");
                    }
                }


                // ====================================================
                // COMMIT
                // ====================================================

                transaction.Commit();
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                }

                throw;
            }
        }


        // ============================================================
        // MAP SALE
        // ============================================================

        private static Sale MapSale(
            MySqlDataReader reader)
        {
            return new Sale
            {
                Id =
                    Convert.ToInt32(
                        reader["id"]),

                PharmacyId =
                    Convert.ToInt32(
                        reader["pharmacy_id"]),

                InvoiceNumber =
                    reader["invoice_no"]?
                        .ToString()
                    ?? string.Empty,

                SaleDate =
                    Convert.ToDateTime(
                        reader["sale_date"]),

                CustomerId =
                    reader["customer_id"] ==
                    DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["customer_id"]),

                TotalAmount =
                    Convert.ToDecimal(
                        reader["total_amount"]),

                DiscountAmount =
                    Convert.ToDecimal(
                        reader["discount_amount"]),

                NetAmount =
                    Convert.ToDecimal(
                        reader["net_amount"]),

                AmountReceived =
                    Convert.ToDecimal(
                        reader["amount_received"]),

                ChangeAmount =
                    Convert.ToDecimal(
                        reader["change_amount"]),

                BalanceAmount =
                    Convert.ToDecimal(
                        reader["balance_amount"]),

                GenerateBill =
                    Convert.ToBoolean(
                        reader["generate_bill"]),

                Items =
                    new List<SaleItem>()
            };
        }
    }
}