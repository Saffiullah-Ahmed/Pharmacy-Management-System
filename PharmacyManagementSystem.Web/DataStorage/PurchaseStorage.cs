using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class PurchaseStorage
    {
        // =========================================================
        // LOAD
        // =========================================================

        public static List<Purchase> Load(
            int pharmacyId)
        {
            var purchases = new List<Purchase>();

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    supplier_name,
                    purchase_date,
                    total_amount
                FROM purchases
                WHERE pharmacy_id = @pharmacy_id
                ORDER BY purchase_date DESC, id DESC;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader =
                    command.ExecuteReader();

                while (reader.Read())
                {
                    var purchase = new Purchase
                    {
                        Id = Convert.ToInt32(
                            reader["id"]
                        ),

                        InvoiceNumber =
                            reader["invoice_no"]?.ToString()
                            ?? string.Empty,

                        SupplierName =
                            reader["supplier_name"]?.ToString()
                            ?? string.Empty,

                        PurchaseDate =
                            Convert.ToDateTime(
                                reader["purchase_date"]
                            ),

                        TotalAmount =
                            Convert.ToDecimal(
                                reader["total_amount"]
                            )
                    };

                    purchases.Add(purchase);
                }

                reader.Close();

                foreach (var purchase in purchases)
                {
                    purchase.Items =
                        GetItems(
                            purchase.Id,
                            pharmacyId
                        );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PurchaseStorage.Load Error: {ex.Message}"
                );
            }

            return purchases;
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public static Purchase? GetById(
            int id,
            int pharmacyId)
        {
            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    supplier_name,
                    purchase_date,
                    total_amount
                FROM purchases
                WHERE id = @id
                  AND pharmacy_id = @pharmacy_id
                LIMIT 1;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@id",
                    id
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader =
                    command.ExecuteReader();

                if (!reader.Read())
                    return null;

                var purchase = new Purchase
                {
                    Id = Convert.ToInt32(
                        reader["id"]
                    ),

                    InvoiceNumber =
                        reader["invoice_no"]?.ToString()
                        ?? string.Empty,

                    SupplierName =
                        reader["supplier_name"]?.ToString()
                        ?? string.Empty,

                    PurchaseDate =
                        Convert.ToDateTime(
                            reader["purchase_date"]
                        ),

                    TotalAmount =
                        Convert.ToDecimal(
                            reader["total_amount"]
                        )
                };

                reader.Close();

                purchase.Items =
                    GetItems(
                        purchase.Id,
                        pharmacyId
                    );

                return purchase;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PurchaseStorage.GetById Error: {ex.Message}"
                );

                return null;
            }
        }

        // =========================================================
        // GET BY INVOICE
        // =========================================================

        public static Purchase? GetByInvoiceNumber(
            string invoiceNumber,
            int pharmacyId)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                return null;

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    invoice_no,
                    supplier_name,
                    purchase_date,
                    total_amount
                FROM purchases
                WHERE invoice_no = @invoice_no
                  AND pharmacy_id = @pharmacy_id
                LIMIT 1;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@invoice_no",
                    invoiceNumber.Trim()
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader =
                    command.ExecuteReader();

                if (!reader.Read())
                    return null;

                var purchase = new Purchase
                {
                    Id = Convert.ToInt32(
                        reader["id"]
                    ),

                    InvoiceNumber =
                        reader["invoice_no"]?.ToString()
                        ?? string.Empty,

                    SupplierName =
                        reader["supplier_name"]?.ToString()
                        ?? string.Empty,

                    PurchaseDate =
                        Convert.ToDateTime(
                            reader["purchase_date"]
                        ),

                    TotalAmount =
                        Convert.ToDecimal(
                            reader["total_amount"]
                        )
                };

                reader.Close();

                purchase.Items =
                    GetItems(
                        purchase.Id,
                        pharmacyId
                    );

                return purchase;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PurchaseStorage.GetByInvoiceNumber Error: {ex.Message}"
                );

                return null;
            }
        }

        // =========================================================
        // GET ITEMS
        // =========================================================

        private static List<PurchaseItem> GetItems(
            int purchaseId,
            int pharmacyId)
        {
            var items = new List<PurchaseItem>();

            const string sql = @"
                SELECT
                    pi.id,
                    pi.purchase_id,
                    pi.medicine_id,
                    pi.quantity,
                    pi.cost_price,
                    pi.total_price,
                    m.name AS medicine_name
                FROM purchase_items pi
                INNER JOIN purchases p
                    ON p.id = pi.purchase_id
                   AND p.pharmacy_id = @pharmacy_id
                INNER JOIN medicines m
                    ON m.id = pi.medicine_id
                   AND m.pharmacy_id = @pharmacy_id
                WHERE pi.purchase_id = @purchase_id
                ORDER BY pi.id ASC;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@purchase_id",
                    purchaseId
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader =
                    command.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(new PurchaseItem
                    {
                        Id = Convert.ToInt32(
                            reader["id"]
                        ),

                        PurchaseId =
                            Convert.ToInt32(
                                reader["purchase_id"]
                            ),

                        MedicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]
                            ),

                        MedicineName =
                            reader["medicine_name"]?.ToString()
                            ?? string.Empty,

                        Quantity =
                            Convert.ToInt32(
                                reader["quantity"]
                            ),

                        CostPrice =
                            Convert.ToDecimal(
                                reader["cost_price"]
                            ),

                        TotalPrice =
                            Convert.ToDecimal(
                                reader["total_price"]
                            )
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PurchaseStorage.GetItems Error: {ex.Message}"
                );
            }

            return items;
        }

        // =========================================================
        // GENERATE PREVIEW INVOICE NUMBER
        // =========================================================

        public static string GenerateInvoiceNumber(
            int pharmacyId)
        {
            const string sql = @"
                SELECT COALESCE(MAX(id), 0) + 1
                FROM purchases
                WHERE pharmacy_id = @pharmacy_id;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                var nextId =
                    Convert.ToInt32(
                        command.ExecuteScalar()
                    );

                return $"PUR-{nextId:D5}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"PurchaseStorage.GenerateInvoiceNumber Error: {ex.Message}"
                );

                return "PUR-00001";
            }
        }

        // =========================================================
        // ADD PURCHASE
        // =========================================================

        public static bool Add(
            Purchase purchase,
            int pharmacyId)
        {
            if (purchase.Items == null ||
                purchase.Items.Count == 0)
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

                foreach (var item in purchase.Items)
                {
                    if (item.MedicineId <= 0 ||
                        item.Quantity <= 0 ||
                        item.CostPrice < 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    item.TotalPrice =
                        item.Quantity *
                        item.CostPrice;

                    totalAmount +=
                        item.TotalPrice;

                    const string medicineCheckSql = @"
                        SELECT id
                        FROM medicines
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id
                        LIMIT 1;
                    ";

                    using var medicineCheck =
                        new MySqlCommand(
                            medicineCheckSql,
                            connection,
                            transaction
                        );

                    medicineCheck.Parameters.AddWithValue(
                        "@medicine_id",
                        item.MedicineId
                    );

                    medicineCheck.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId
                    );

                    var exists =
                        medicineCheck.ExecuteScalar();

                    if (exists == null)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                // -------------------------------------------------
                // Insert purchase using temporary invoice.
                // Final invoice is based on database-generated ID.
                // -------------------------------------------------

                const string insertPurchaseSql = @"
                    INSERT INTO purchases
                    (
                        pharmacy_id,
                        invoice_no,
                        supplier_name,
                        purchase_date,
                        total_amount
                    )
                    VALUES
                    (
                        @pharmacy_id,
                        @invoice_no,
                        @supplier_name,
                        @purchase_date,
                        @total_amount
                    );

                    SELECT LAST_INSERT_ID();
                ";

                using var insertPurchase =
                    new MySqlCommand(
                        insertPurchaseSql,
                        connection,
                        transaction
                    );

                insertPurchase.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                insertPurchase.Parameters.AddWithValue(
                    "@invoice_no",
                    $"TEMP-{Guid.NewGuid():N}"
                );

                insertPurchase.Parameters.AddWithValue(
                    "@supplier_name",
                    purchase.SupplierName.Trim()
                );

                insertPurchase.Parameters.AddWithValue(
                    "@purchase_date",
                    purchase.PurchaseDate
                );

                insertPurchase.Parameters.AddWithValue(
                    "@total_amount",
                    totalAmount
                );

                var purchaseId =
                    Convert.ToInt32(
                        insertPurchase.ExecuteScalar()
                    );

                var invoiceNumber =
                    $"PUR-{purchaseId:D5}";

                const string updateInvoiceSql = @"
                    UPDATE purchases
                    SET invoice_no = @invoice_no
                    WHERE id = @id
                      AND pharmacy_id = @pharmacy_id;
                ";

                using var updateInvoice =
                    new MySqlCommand(
                        updateInvoiceSql,
                        connection,
                        transaction
                    );

                updateInvoice.Parameters.AddWithValue(
                    "@invoice_no",
                    invoiceNumber
                );

                updateInvoice.Parameters.AddWithValue(
                    "@id",
                    purchaseId
                );

                updateInvoice.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                updateInvoice.ExecuteNonQuery();

                // -------------------------------------------------
                // Insert items + increase stock
                // -------------------------------------------------

                foreach (var item in purchase.Items)
                {
                    const string insertItemSql = @"
                        INSERT INTO purchase_items
                        (
                            purchase_id,
                            medicine_id,
                            quantity,
                            cost_price,
                            total_price
                        )
                        VALUES
                        (
                            @purchase_id,
                            @medicine_id,
                            @quantity,
                            @cost_price,
                            @total_price
                        );
                    ";

                    using var insertItem =
                        new MySqlCommand(
                            insertItemSql,
                            connection,
                            transaction
                        );

                    insertItem.Parameters.AddWithValue(
                        "@purchase_id",
                        purchaseId
                    );

                    insertItem.Parameters.AddWithValue(
                        "@medicine_id",
                        item.MedicineId
                    );

                    insertItem.Parameters.AddWithValue(
                        "@quantity",
                        item.Quantity
                    );

                    insertItem.Parameters.AddWithValue(
                        "@cost_price",
                        item.CostPrice
                    );

                    insertItem.Parameters.AddWithValue(
                        "@total_price",
                        item.TotalPrice
                    );

                    insertItem.ExecuteNonQuery();

                    const string stockSql = @"
                        UPDATE medicines
                        SET quantity = quantity + @quantity
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id;
                    ";

                    using var stockCommand =
                        new MySqlCommand(
                            stockSql,
                            connection,
                            transaction
                        );

                    stockCommand.Parameters.AddWithValue(
                        "@quantity",
                        item.Quantity
                    );

                    stockCommand.Parameters.AddWithValue(
                        "@medicine_id",
                        item.MedicineId
                    );

                    stockCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId
                    );

                    if (stockCommand.ExecuteNonQuery() != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                }

                Console.WriteLine(
                    $"PurchaseStorage.Add Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // UPDATE PURCHASE
        // =========================================================

        public static bool Update(
            Purchase purchase,
            int pharmacyId)
        {
            if (purchase.Items == null ||
                purchase.Items.Count == 0)
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
                const string purchaseCheckSql = @"
                    SELECT id
                    FROM purchases
                    WHERE id = @id
                      AND pharmacy_id = @pharmacy_id
                    LIMIT 1;
                ";

                using var purchaseCheck =
                    new MySqlCommand(
                        purchaseCheckSql,
                        connection,
                        transaction
                    );

                purchaseCheck.Parameters.AddWithValue(
                    "@id",
                    purchase.Id
                );

                purchaseCheck.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                if (purchaseCheck.ExecuteScalar() == null)
                {
                    transaction.Rollback();
                    return false;
                }

                var oldItems =
                    GetItemsForTransaction(
                        purchase.Id,
                        pharmacyId,
                        connection,
                        transaction
                    );

                // -------------------------------------------------
                // Validate new items
                // -------------------------------------------------

                foreach (var item in purchase.Items)
                {
                    if (item.MedicineId <= 0 ||
                        item.Quantity <= 0 ||
                        item.CostPrice < 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    const string medicineCheckSql = @"
                        SELECT id
                        FROM medicines
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id
                        LIMIT 1;
                    ";

                    using var medicineCheck =
                        new MySqlCommand(
                            medicineCheckSql,
                            connection,
                            transaction
                        );

                    medicineCheck.Parameters.AddWithValue(
                        "@medicine_id",
                        item.MedicineId
                    );

                    medicineCheck.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId
                    );

                    if (medicineCheck.ExecuteScalar() == null)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                // -------------------------------------------------
                // Calculate stock differences
                // -------------------------------------------------

                var oldQuantities =
                    AggregateQuantities(oldItems);

                var newQuantities =
                    AggregateQuantities(purchase.Items);

                var medicineIds =
                    oldQuantities.Keys
                        .Union(newQuantities.Keys)
                        .Distinct()
                        .ToList();

                foreach (var medicineId in medicineIds)
                {
                    var oldQuantity =
                        oldQuantities.TryGetValue(
                            medicineId,
                            out var oldValue)
                            ? oldValue
                            : 0;

                    var newQuantity =
                        newQuantities.TryGetValue(
                            medicineId,
                            out var newValue)
                            ? newValue
                            : 0;

                    var difference =
                        newQuantity - oldQuantity;

                    if (difference == 0)
                        continue;

                    if (difference < 0)
                    {
                        var amountToRemove =
                            Math.Abs(difference);

                        const string reduceSql = @"
                            UPDATE medicines
                            SET quantity = quantity - @amount
                            WHERE id = @medicine_id
                              AND pharmacy_id = @pharmacy_id
                              AND quantity >= @amount;
                        ";

                        using var reduceCommand =
                            new MySqlCommand(
                                reduceSql,
                                connection,
                                transaction
                            );

                        reduceCommand.Parameters.AddWithValue(
                            "@amount",
                            amountToRemove
                        );

                        reduceCommand.Parameters.AddWithValue(
                            "@medicine_id",
                            medicineId
                        );

                        reduceCommand.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        if (reduceCommand.ExecuteNonQuery() != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        const string increaseSql = @"
                            UPDATE medicines
                            SET quantity = quantity + @amount
                            WHERE id = @medicine_id
                              AND pharmacy_id = @pharmacy_id;
                        ";

                        using var increaseCommand =
                            new MySqlCommand(
                                increaseSql,
                                connection,
                                transaction
                            );

                        increaseCommand.Parameters.AddWithValue(
                            "@amount",
                            difference
                        );

                        increaseCommand.Parameters.AddWithValue(
                            "@medicine_id",
                            medicineId
                        );

                        increaseCommand.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        if (increaseCommand.ExecuteNonQuery() != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }

                decimal totalAmount = 0;

                foreach (var item in purchase.Items)
                {
                    item.TotalPrice =
                        item.Quantity *
                        item.CostPrice;

                    totalAmount +=
                        item.TotalPrice;
                }

                const string updatePurchaseSql = @"
                    UPDATE purchases
                    SET
                        supplier_name = @supplier_name,
                        purchase_date = @purchase_date,
                        total_amount = @total_amount
                    WHERE id = @id
                      AND pharmacy_id = @pharmacy_id;
                ";

                using var updatePurchase =
                    new MySqlCommand(
                        updatePurchaseSql,
                        connection,
                        transaction
                    );

                updatePurchase.Parameters.AddWithValue(
                    "@supplier_name",
                    purchase.SupplierName.Trim()
                );

                updatePurchase.Parameters.AddWithValue(
                    "@purchase_date",
                    purchase.PurchaseDate
                );

                updatePurchase.Parameters.AddWithValue(
                    "@total_amount",
                    totalAmount
                );

                updatePurchase.Parameters.AddWithValue(
                    "@id",
                    purchase.Id
                );

                updatePurchase.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                if (updatePurchase.ExecuteNonQuery() != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                // -------------------------------------------------
                // Replace purchase items
                // -------------------------------------------------

                const string deleteItemsSql = @"
                    DELETE FROM purchase_items
                    WHERE purchase_id = @purchase_id;
                ";

                using var deleteItems =
                    new MySqlCommand(
                        deleteItemsSql,
                        connection,
                        transaction
                    );

                deleteItems.Parameters.AddWithValue(
                    "@purchase_id",
                    purchase.Id
                );

                deleteItems.ExecuteNonQuery();

                foreach (var item in purchase.Items)
                {
                    const string insertItemSql = @"
                        INSERT INTO purchase_items
                        (
                            purchase_id,
                            medicine_id,
                            quantity,
                            cost_price,
                            total_price
                        )
                        VALUES
                        (
                            @purchase_id,
                            @medicine_id,
                            @quantity,
                            @cost_price,
                            @total_price
                        );
                    ";

                    using var insertItem =
                        new MySqlCommand(
                            insertItemSql,
                            connection,
                            transaction
                        );

                    insertItem.Parameters.AddWithValue(
                        "@purchase_id",
                        purchase.Id
                    );

                    insertItem.Parameters.AddWithValue(
                        "@medicine_id",
                        item.MedicineId
                    );

                    insertItem.Parameters.AddWithValue(
                        "@quantity",
                        item.Quantity
                    );

                    insertItem.Parameters.AddWithValue(
                        "@cost_price",
                        item.CostPrice
                    );

                    insertItem.Parameters.AddWithValue(
                        "@total_price",
                        item.TotalPrice
                    );

                    insertItem.ExecuteNonQuery();
                }

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                }

                Console.WriteLine(
                    $"PurchaseStorage.Update Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // DELETE PURCHASE
        // =========================================================

        public static bool Delete(
            int purchaseId,
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                const string purchaseCheckSql = @"
                    SELECT id
                    FROM purchases
                    WHERE id = @id
                      AND pharmacy_id = @pharmacy_id
                    LIMIT 1;
                ";

                using var purchaseCheck =
                    new MySqlCommand(
                        purchaseCheckSql,
                        connection,
                        transaction
                    );

                purchaseCheck.Parameters.AddWithValue(
                    "@id",
                    purchaseId
                );

                purchaseCheck.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                if (purchaseCheck.ExecuteScalar() == null)
                {
                    transaction.Rollback();
                    return false;
                }

                var items =
                    GetItemsForTransaction(
                        purchaseId,
                        pharmacyId,
                        connection,
                        transaction
                    );

                var quantities =
                    AggregateQuantities(items);

                // -------------------------------------------------
                // Remove stock safely
                // -------------------------------------------------

                foreach (var entry in quantities)
                {
                    const string stockSql = @"
                        UPDATE medicines
                        SET quantity = quantity - @quantity
                        WHERE id = @medicine_id
                          AND pharmacy_id = @pharmacy_id
                          AND quantity >= @quantity;
                    ";

                    using var stockCommand =
                        new MySqlCommand(
                            stockSql,
                            connection,
                            transaction
                        );

                    stockCommand.Parameters.AddWithValue(
                        "@quantity",
                        entry.Value
                    );

                    stockCommand.Parameters.AddWithValue(
                        "@medicine_id",
                        entry.Key
                    );

                    stockCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId
                    );

                    if (stockCommand.ExecuteNonQuery() != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                const string deleteItemsSql = @"
                    DELETE FROM purchase_items
                    WHERE purchase_id = @purchase_id;
                ";

                using var deleteItems =
                    new MySqlCommand(
                        deleteItemsSql,
                        connection,
                        transaction
                    );

                deleteItems.Parameters.AddWithValue(
                    "@purchase_id",
                    purchaseId
                );

                deleteItems.ExecuteNonQuery();

                const string deletePurchaseSql = @"
                    DELETE FROM purchases
                    WHERE id = @id
                      AND pharmacy_id = @pharmacy_id;
                ";

                using var deletePurchase =
                    new MySqlCommand(
                        deletePurchaseSql,
                        connection,
                        transaction
                    );

                deletePurchase.Parameters.AddWithValue(
                    "@id",
                    purchaseId
                );

                deletePurchase.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                if (deletePurchase.ExecuteNonQuery() != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                }

                Console.WriteLine(
                    $"PurchaseStorage.Delete Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // GET ITEMS INSIDE TRANSACTION
        // =========================================================

        private static List<PurchaseItem>
            GetItemsForTransaction(
                int purchaseId,
                int pharmacyId,
                MySqlConnection connection,
                MySqlTransaction transaction)
        {
            var items = new List<PurchaseItem>();

            const string sql = @"
                SELECT
                    pi.id,
                    pi.purchase_id,
                    pi.medicine_id,
                    pi.quantity,
                    pi.cost_price,
                    pi.total_price,
                    m.name AS medicine_name
                FROM purchase_items pi
                INNER JOIN purchases p
                    ON p.id = pi.purchase_id
                   AND p.pharmacy_id = @pharmacy_id
                INNER JOIN medicines m
                    ON m.id = pi.medicine_id
                   AND m.pharmacy_id = @pharmacy_id
                WHERE pi.purchase_id = @purchase_id
                ORDER BY pi.id ASC;
            ";

            using var command =
                new MySqlCommand(
                    sql,
                    connection,
                    transaction
                );

            command.Parameters.AddWithValue(
                "@purchase_id",
                purchaseId
            );

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId
            );

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new PurchaseItem
                {
                    Id = Convert.ToInt32(
                        reader["id"]
                    ),

                    PurchaseId =
                        Convert.ToInt32(
                            reader["purchase_id"]
                        ),

                    MedicineId =
                        Convert.ToInt32(
                            reader["medicine_id"]
                        ),

                    MedicineName =
                        reader["medicine_name"]?.ToString()
                        ?? string.Empty,

                    Quantity =
                        Convert.ToInt32(
                            reader["quantity"]
                        ),

                    CostPrice =
                        Convert.ToDecimal(
                            reader["cost_price"]
                        ),

                    TotalPrice =
                        Convert.ToDecimal(
                            reader["total_price"]
                        )
                });
            }

            return items;
        }

        // =========================================================
        // AGGREGATE QUANTITIES
        // =========================================================

        private static Dictionary<int, int>
            AggregateQuantities(
                IEnumerable<PurchaseItem> items)
        {
            var result =
                new Dictionary<int, int>();

            foreach (var item in items)
            {
                if (result.ContainsKey(item.MedicineId))
                {
                    result[item.MedicineId] +=
                        item.Quantity;
                }
                else
                {
                    result[item.MedicineId] =
                        item.Quantity;
                }
            }

            return result;
        }
    }
}