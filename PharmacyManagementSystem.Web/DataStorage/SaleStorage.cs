using MySql.Data.MySqlClient;

using PharmacyManagementSystem.Web.Models;

using PharmacyManagementSystem.Web.Database;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class SaleStorage
    {
        // ==========================================================
        // LOAD SALES FOR CURRENT PHARMACY
        // ==========================================================

        public static List<Sale> Load(
            int pharmacyId)
        {
            var sales =
                new List<Sale>();

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    customer_id,
                    sale_date,
                    total_amount,
                    discount_amount,
                    net_amount,
                    amount_received,
                    change_amount,
                    balance_amount,
                    generate_bill
                FROM sales
                WHERE pharmacy_id = @pharmacy_id
                ORDER BY sale_date DESC, id DESC;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                sales.Add(
                    MapSale(reader));
            }

            reader.Close();

            foreach (var sale in sales)
            {
                sale.Items =
                    GetItems(
                        sale.Id,
                        pharmacyId);
            }

            return sales;
        }


        // ==========================================================
        // GET SALE BY ID
        // ==========================================================

        public static Sale? GetById(
            int id,
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    customer_id,
                    sale_date,
                    total_amount,
                    discount_amount,
                    net_amount,
                    amount_received,
                    change_amount,
                    balance_amount,
                    generate_bill
                FROM sales
                WHERE id = @id
                  AND pharmacy_id = @pharmacy_id
                LIMIT 1;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@id",
                id);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            using var reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            var sale =
                MapSale(reader);

            reader.Close();

            sale.Items =
                GetItems(
                    sale.Id,
                    pharmacyId);

            return sale;
        }


        // ==========================================================
        // GET SALE BY INVOICE NUMBER
        // ==========================================================

        public static Sale? GetByInvoiceNumber(
            string invoiceNumber,
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    customer_id,
                    sale_date,
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
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@invoice_no",
                invoiceNumber.Trim());

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            using var reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            var sale =
                MapSale(reader);

            reader.Close();

            sale.Items =
                GetItems(
                    sale.Id,
                    pharmacyId);

            return sale;
        }


        // ==========================================================
        // SERVER-SIDE MEDICINE SEARCH
        //
        // This method intentionally returns only a small number
        // of medicines instead of loading the entire medicines
        // table into the browser.
        // ==========================================================

        public static List<Medicine> SearchMedicines(
            string search,
            int pharmacyId,
            int limit = 20)
        {
            var medicines =
                new List<Medicine>();

            if (pharmacyId <= 0)
                return medicines;

            if (string.IsNullOrWhiteSpace(search))
                return medicines;

            search =
                search.Trim();

            if (search.Length < 2)
                return medicines;

            // Keep the limit controlled by the application.
            // The controller currently sends 20.
            limit =
                Math.Clamp(
                    limit,
                    1,
                    50);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    name,
                    price,
                    quantity
                FROM medicines
                WHERE pharmacy_id = @pharmacy_id
                  AND
                  (
                      name LIKE @search
                      OR CAST(id AS CHAR) LIKE @search
                  )
                ORDER BY
                    CASE
                        WHEN name = @exact_name
                            THEN 0
                        WHEN name LIKE @starts_with
                            THEN 1
                        ELSE 2
                    END,
                    name ASC
                LIMIT @limit;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@search",
                $"%{search}%");

            command.Parameters.AddWithValue(
                "@exact_name",
                search);

            command.Parameters.AddWithValue(
                "@starts_with",
                $"{search}%");

            command.Parameters.AddWithValue(
                "@limit",
                limit);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                medicines.Add(
                    new Medicine
                    {
                        Id =
                            Convert.ToInt32(
                                reader["id"]),

                        PharmacyId =
                            Convert.ToInt32(
                                reader["pharmacy_id"]),

                        Name =
                            reader["name"]
                                ?.ToString()
                            ?? string.Empty,

                        Price =
                            Convert.ToDecimal(
                                reader["price"]),

                        Quantity =
                            Convert.ToInt32(
                                reader["quantity"])
                    });
            }

            return medicines;
        }


        // ==========================================================
        // GET SELECTED MEDICINES BY IDS
        //
        // Used when validation fails and the Create Sale page
        // needs to rebuild the selected medicine information.
        //
        // IMPORTANT:
        // pharmacy_id is always included.
        // ==========================================================

        public static List<Medicine> GetMedicinesByIds(
            IEnumerable<int> medicineIds,
            int pharmacyId)
        {
            var ids =
                medicineIds
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

            var medicines =
                new List<Medicine>();

            if (pharmacyId <= 0 ||
                ids.Count == 0)
            {
                return medicines;
            }

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            var parameterNames =
                new List<string>();

            using var command =
                new MySqlCommand();

            command.Connection =
                connection;

            for (int i = 0;
                 i < ids.Count;
                 i++)
            {
                string parameterName =
                    $"@medicine_id_{i}";

                parameterNames.Add(
                    parameterName);

                command.Parameters.AddWithValue(
                    parameterName,
                    ids[i]);
            }

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.CommandText = $@"
                SELECT
                    id,
                    pharmacy_id,
                    name,
                    price,
                    quantity
                FROM medicines
                WHERE pharmacy_id = @pharmacy_id
                  AND id IN
                  (
                      {string.Join(
                          ", ",
                          parameterNames)}
                  )
                ORDER BY name ASC;";

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                medicines.Add(
                    new Medicine
                    {
                        Id =
                            Convert.ToInt32(
                                reader["id"]),

                        PharmacyId =
                            Convert.ToInt32(
                                reader["pharmacy_id"]),

                        Name =
                            reader["name"]
                                ?.ToString()
                            ?? string.Empty,

                        Price =
                            Convert.ToDecimal(
                                reader["price"]),

                        Quantity =
                            Convert.ToInt32(
                                reader["quantity"])
                    });
            }

            return medicines;
        }


        // ==========================================================
        // GET SALE ITEMS
        //
        // Actual database structure:
        //
        // sale_items
        //     id
        //     sale_id
        //     medicine_id
        //     quantity
        //     price
        //     total
        //
        // Medicine name comes from:
        //
        // medicines.name
        // ==========================================================

        private static List<SaleItem> GetItems(
            int saleId,
            int pharmacyId)
        {
            var items =
                new List<SaleItem>();

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    si.id,
                    si.sale_id,
                    si.medicine_id,
                    m.name AS medicine_name,
                    si.quantity,
                    si.price AS unit_price,
                    si.total AS total_price
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
                    sql,
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
                            reader["medicine_name"]
                                ?.ToString()
                            ?? string.Empty,

                        Quantity =
                            Convert.ToInt32(
                                reader["quantity"]),

                        UnitPrice =
                            Convert.ToDecimal(
                                reader["unit_price"]),

                        TotalPrice =
                            Convert.ToDecimal(
                                reader["total_price"])
                    });
            }

            return items;
        }


        // ==========================================================
        // GENERATE INVOICE NUMBER
        // ==========================================================

        public static string GenerateInvoiceNumber(
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COALESCE(MAX(id), 0) + 1
                FROM sales
                WHERE pharmacy_id = @pharmacy_id;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            int nextNumber =
                Convert.ToInt32(
                    command.ExecuteScalar());

            return
                $"INV-{nextNumber:D5}";
        }


        // ==========================================================
        // ADD SALE
        // ==========================================================

        public static bool Add(
            Sale sale,
            int pharmacyId)
        {
            if (sale == null)
                return false;

            if (pharmacyId <= 0)
                return false;

            if (sale.Items == null ||
                sale.Items.Count == 0)
            {
                return false;
            }

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                decimal totalAmount = 0;


                // --------------------------------------------------
                // VALIDATE EVERY MEDICINE
                // --------------------------------------------------

                foreach (var item in sale.Items)
                {
                    if (!item.MedicineId.HasValue ||
                        item.MedicineId.Value <= 0)
                    {
                        throw new Exception(
                            "Please select a valid medicine.");
                    }

                    int medicineId =
                        item.MedicineId.Value;


                    if (!item.Quantity.HasValue ||
                        item.Quantity.Value <= 0)
                    {
                        throw new Exception(
                            "Medicine quantity must be greater than zero.");
                    }

                    int quantity =
                        item.Quantity.Value;


                    const string medicineSql = @"
                        SELECT
                            price,
                            quantity
                        FROM medicines
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id
                        FOR UPDATE;";

                    using var medicineCommand =
                        new MySqlCommand(
                            medicineSql,
                            connection,
                            transaction);

                    medicineCommand.Parameters.AddWithValue(
                        "@medicine_id",
                        medicineId);

                    medicineCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId);

                    using var reader =
                        medicineCommand.ExecuteReader();

                    if (!reader.Read())
                    {
                        reader.Close();

                        throw new Exception(
                            "One of the selected medicines does not belong to this pharmacy.");
                    }

                    decimal price =
                        Convert.ToDecimal(
                            reader["price"]);

                    int stock =
                        Convert.ToInt32(
                            reader["quantity"]);

                    reader.Close();


                    if (stock < quantity)
                    {
                        throw new Exception(
                            $"Insufficient stock for medicine ID {medicineId}. Available stock: {stock}.");
                    }


                    // --------------------------------------------------
                    // ALWAYS USE DATABASE PRICE
                    // --------------------------------------------------

                    item.UnitPrice =
                        price;

                    item.TotalPrice =
                        price * quantity;

                    totalAmount +=
                        item.TotalPrice;
                }


                // --------------------------------------------------
                // CALCULATE SALE TOTALS
                // --------------------------------------------------

                sale.TotalAmount =
                    totalAmount;

                if (sale.DiscountAmount < 0)
                    sale.DiscountAmount = 0;

                if (sale.DiscountAmount >
                    sale.TotalAmount)
                {
                    sale.DiscountAmount =
                        sale.TotalAmount;
                }

                sale.NetAmount =
                    sale.TotalAmount -
                    sale.DiscountAmount;


                // --------------------------------------------------
                // PAYMENT CALCULATION
                // --------------------------------------------------

                if (sale.AmountReceived < 0)
                    sale.AmountReceived = 0;


                if (sale.AmountReceived >=
                    sale.NetAmount)
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


                // --------------------------------------------------
                // SET PHARMACY
                // --------------------------------------------------

                sale.PharmacyId =
                    pharmacyId;


                // --------------------------------------------------
                // GENERATE INVOICE
                // --------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    sale.InvoiceNumber))
                {
                    sale.InvoiceNumber =
                        GenerateInvoiceNumber(
                            pharmacyId);
                }


                // --------------------------------------------------
                // SALE DATE
                // --------------------------------------------------

                if (sale.SaleDate == default)
                {
                    sale.SaleDate =
                        DateTime.Now;
                }


                // --------------------------------------------------
                // INSERT SALE HEADER
                // --------------------------------------------------

                const string saleSql = @"
                    INSERT INTO sales
                    (
                        pharmacy_id,
                        invoice_no,
                        customer_id,
                        sale_date,
                        total_amount,
                        discount_amount,
                        net_amount,
                        amount_received,
                        change_amount,
                        balance_amount,
                        generate_bill
                    )
                    VALUES
                    (
                        @pharmacy_id,
                        @invoice_no,
                        @customer_id,
                        @sale_date,
                        @total_amount,
                        @discount_amount,
                        @net_amount,
                        @amount_received,
                        @change_amount,
                        @balance_amount,
                        @generate_bill
                    );";

                using var saleCommand =
                    new MySqlCommand(
                        saleSql,
                        connection,
                        transaction);

                saleCommand.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId);

                saleCommand.Parameters.AddWithValue(
                    "@invoice_no",
                    sale.InvoiceNumber);

                saleCommand.Parameters.AddWithValue(
                    "@customer_id",
                    sale.CustomerId.HasValue
                        ? sale.CustomerId.Value
                        : DBNull.Value);

                saleCommand.Parameters.AddWithValue(
                    "@sale_date",
                    sale.SaleDate);

                saleCommand.Parameters.AddWithValue(
                    "@total_amount",
                    sale.TotalAmount);

                saleCommand.Parameters.AddWithValue(
                    "@discount_amount",
                    sale.DiscountAmount);

                saleCommand.Parameters.AddWithValue(
                    "@net_amount",
                    sale.NetAmount);

                saleCommand.Parameters.AddWithValue(
                    "@amount_received",
                    sale.AmountReceived);

                saleCommand.Parameters.AddWithValue(
                    "@change_amount",
                    sale.ChangeAmount);

                saleCommand.Parameters.AddWithValue(
                    "@balance_amount",
                    sale.BalanceAmount);

                saleCommand.Parameters.AddWithValue(
                    "@generate_bill",
                    sale.GenerateBill);

                saleCommand.ExecuteNonQuery();

                sale.Id =
                    Convert.ToInt32(
                        saleCommand.LastInsertedId);


                // --------------------------------------------------
                // INSERT SALE ITEMS + REDUCE STOCK
                // --------------------------------------------------

                foreach (var item in sale.Items)
                {
                    if (!item.MedicineId.HasValue)
                    {
                        throw new Exception(
                            "Medicine is required for every sale item.");
                    }

                    int medicineId =
                        item.MedicineId.Value;


                    if (!item.Quantity.HasValue ||
                        item.Quantity.Value <= 0)
                    {
                        throw new Exception(
                            "Medicine quantity must be greater than zero.");
                    }

                    int quantity =
                        item.Quantity.Value;


                    // --------------------------------------------------
                    // GET ACTUAL MEDICINE NAME
                    // --------------------------------------------------

                    const string nameSql = @"
                        SELECT
                            name
                        FROM medicines
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id
                        LIMIT 1;";

                    string medicineName;

                    using (
                        var nameCommand =
                            new MySqlCommand(
                                nameSql,
                                connection,
                                transaction))
                    {
                        nameCommand.Parameters.AddWithValue(
                            "@medicine_id",
                            medicineId);

                        nameCommand.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);

                        medicineName =
                            nameCommand.ExecuteScalar()
                                ?.ToString()
                            ?? item.MedicineName;
                    }


                    item.MedicineName =
                        medicineName;


                    // --------------------------------------------------
                    // INSERT SALE ITEM
                    // --------------------------------------------------

                    const string itemSql = @"
                        INSERT INTO sale_items
                        (
                            sale_id,
                            medicine_id,
                            quantity,
                            price,
                            total
                        )
                        VALUES
                        (
                            @sale_id,
                            @medicine_id,
                            @quantity,
                            @price,
                            @total
                        );";

                    using var itemCommand =
                        new MySqlCommand(
                            itemSql,
                            connection,
                            transaction);

                    itemCommand.Parameters.AddWithValue(
                        "@sale_id",
                        sale.Id);

                    itemCommand.Parameters.AddWithValue(
                        "@medicine_id",
                        medicineId);

                    itemCommand.Parameters.AddWithValue(
                        "@quantity",
                        quantity);

                    itemCommand.Parameters.AddWithValue(
                        "@price",
                        item.UnitPrice);

                    itemCommand.Parameters.AddWithValue(
                        "@total",
                        item.TotalPrice);

                    itemCommand.ExecuteNonQuery();


                    // --------------------------------------------------
                    // REDUCE STOCK
                    // --------------------------------------------------

                    const string stockSql = @"
                        UPDATE medicines
                        SET quantity =
                            quantity - @quantity
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id
                          AND quantity >= @quantity;";

                    using var stockCommand =
                        new MySqlCommand(
                            stockSql,
                            connection,
                            transaction);

                    stockCommand.Parameters.AddWithValue(
                        "@quantity",
                        quantity);

                    stockCommand.Parameters.AddWithValue(
                        "@medicine_id",
                        medicineId);

                    stockCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId);

                    int affected =
                        stockCommand.ExecuteNonQuery();

                    if (affected != 1)
                    {
                        throw new Exception(
                            $"Unable to update stock for medicine ID {medicineId}.");
                    }
                }


                // --------------------------------------------------
                // COMMIT
                // --------------------------------------------------

                transaction.Commit();

                return true;
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // Ignore rollback failure.
                }

                throw;
            }
        }


        // ==========================================================
        // GET ITEMS FOR CREDIT
        // ==========================================================

        public static List<SaleItem> GetItemsForCredit(
            int saleId,
            int pharmacyId)
        {
            return GetItems(
                saleId,
                pharmacyId);
        }


        // ==========================================================
        // MAP SALE
        // ==========================================================

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
                    reader["invoice_no"]
                        ?.ToString()
                    ?? string.Empty,

                CustomerId =
                    reader["customer_id"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["customer_id"]),

                SaleDate =
                    Convert.ToDateTime(
                        reader["sale_date"]),

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
                        reader["generate_bill"])
            };
        }
    }
}