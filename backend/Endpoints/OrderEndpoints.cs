using Backend.Contracts;
using Backend.Data;
using Microsoft.Data.Sqlite;

namespace Backend.Endpoints;

public static class OrderEndpoints
{
    private static readonly HashSet<string> AllowedOrderStatuses = new(StringComparer.Ordinal)
    {
        "Mới",
        "Đang chuẩn bị",
        "Đang giao",
        "Đã giao",
        "Đã hủy"
    };

    private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.Ordinal)
    {
        "Tiền mặt",
        "Thẻ",
        "MoMo",
        "VNPay",
        "ZaloPay"
    };

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", (OrderCreateRequest request, Db db, HttpRequest httpRequest) =>
        {
            if (request.Items.Count == 0)
            {
                return Results.BadRequest(new { message = "Order must contain at least one item." });
            }

            if (string.IsNullOrWhiteSpace(request.PaymentMethod) ||
                !AllowedPaymentMethods.Contains(request.PaymentMethod.Trim()))
            {
                return Results.BadRequest(new { message = "Invalid payment method." });
            }

            using var connection = db.OpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                using (var userCmd = connection.CreateCommand())
                {
                    userCmd.Transaction = transaction;
                    userCmd.CommandText = """
                        SELECT IsActive
                        FROM Users
                        WHERE UserId = $id;
                        """;
                    userCmd.Parameters.AddWithValue("$id", request.UserId);
                    var isActive = userCmd.ExecuteScalar() as long?;
                    if (!isActive.HasValue)
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = "User not found." });
                    }
                    if (isActive.Value == 0)
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = "Tài khoản đã bị khóa." });
                    }
                }

                var idempotencyKey = httpRequest.Headers["Idempotency-Key"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    using var existingCmd = connection.CreateCommand();
                    existingCmd.Transaction = transaction;
                    existingCmd.CommandText = """
                        SELECT OrderId, OrderCode
                        FROM Orders
                        WHERE IdempotencyKey = $key;
                        """;
                    existingCmd.Parameters.AddWithValue("$key", idempotencyKey);
                    using var existingReader = existingCmd.ExecuteReader();
                    if (existingReader.Read())
                    {
                        transaction.Commit();
                        return Results.Ok(new
                        {
                            OrderId = existingReader.GetInt32(existingReader.GetOrdinal("OrderId")),
                            OrderCode = existingReader.GetString(existingReader.GetOrdinal("OrderCode")),
                        });
                    }
                }

                var orderCode = $"FF-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
                var items = new List<(int ProductId, string Name, double Price, int Quantity, double LineTotal)>();
                var normalizedItems = new Dictionary<int, int>();
                double subTotal = 0;

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = $"Invalid quantity for product {item.ProductId}." });
                    }

                    if (!normalizedItems.TryAdd(item.ProductId, item.Quantity))
                    {
                        normalizedItems[item.ProductId] += item.Quantity;
                    }
                }

                foreach (var item in normalizedItems)
                {
                    using var productCmd = connection.CreateCommand();
                    productCmd.Transaction = transaction;
                    productCmd.CommandText = """
                        SELECT Name, Price, StockQty, IsAvailable
                        FROM Product
                        WHERE ProductId = $id;
                        """;
                    productCmd.Parameters.AddWithValue("$id", item.Key);

                    using var reader = productCmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = $"Product {item.Key} not found." });
                    }

                    if (reader.GetInt32(reader.GetOrdinal("IsAvailable")) == 0)
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = $"Product {item.Key} is not available." });
                    }

                    var stockQty = reader.GetInt32(reader.GetOrdinal("StockQty"));
                    if (item.Value > stockQty)
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = $"Invalid quantity for product {item.Key}." });
                    }

                    var name = reader.GetString(reader.GetOrdinal("Name"));
                    var price = reader.GetDouble(reader.GetOrdinal("Price"));
                    if (price < 0)
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = $"Invalid price for product {item.Key}." });
                    }
                    var lineTotal = price * item.Value;
                    subTotal += lineTotal;

                    items.Add((item.Key, name, price, item.Value, lineTotal));
                }

                var discountTotal = 0.0;
                var deliveryFee = 15000.0;
                var promoCode = request.PromoCode?.Trim().ToUpperInvariant();
                var comboDiscount = Math.Max(0, request.ComboDiscount ?? 0);

                if (!string.IsNullOrWhiteSpace(promoCode))
                {
                    using var promoCmd = connection.CreateCommand();
                    promoCmd.Transaction = transaction;
                    promoCmd.CommandText = """
                        SELECT Type, Value, UsageLimit, UsedCount, ExpiresAt, IsActive
                        FROM PromoCode
                        WHERE Code = $code;
                        """;
                    promoCmd.Parameters.AddWithValue("$code", promoCode);

                    using var promoReader = promoCmd.ExecuteReader();
                    if (promoReader.Read())
                    {
                        var isActive = promoReader.GetInt32(promoReader.GetOrdinal("IsActive")) == 1;
                        var expiresAt = promoReader.GetString(promoReader.GetOrdinal("ExpiresAt"));
                        var usageLimit = promoReader.IsDBNull(promoReader.GetOrdinal("UsageLimit"))
                            ? (int?)null
                            : promoReader.GetInt32(promoReader.GetOrdinal("UsageLimit"));
                        var usedCount = promoReader.GetInt32(promoReader.GetOrdinal("UsedCount"));

                        if (!isActive)
                        {
                            transaction.Rollback();
                            return Results.BadRequest(new { message = "Mã giảm giá đang tạm ngưng." });
                        }

                        if (DateTime.TryParse(expiresAt, out var expires) && expires < DateTime.Now)
                        {
                            transaction.Rollback();
                            return Results.BadRequest(new { message = "Mã giảm giá đã hết hạn." });
                        }

                        if (usageLimit.HasValue && usedCount >= usageLimit.Value)
                        {
                            transaction.Rollback();
                            return Results.BadRequest(new { message = "Mã giảm giá đã hết lượt sử dụng." });
                        }

                        var type = promoReader.GetString(promoReader.GetOrdinal("Type"));
                        var value = promoReader.GetDouble(promoReader.GetOrdinal("Value"));
                        if (type == "Phần trăm")
                        {
                            discountTotal = Math.Min(subTotal, Math.Round(subTotal * (value / 100.0), 0));
                        }
                        else if (type == "Cố định")
                        {
                            discountTotal = Math.Min(subTotal, value);
                        }

                        promoReader.Close();

                        using var promoUpdate = connection.CreateCommand();
                        promoUpdate.Transaction = transaction;
                        promoUpdate.CommandText = """
                            UPDATE PromoCode
                            SET UsedCount = UsedCount + 1
                            WHERE Code = $code;
                            """;
                        promoUpdate.Parameters.AddWithValue("$code", promoCode);
                        promoUpdate.ExecuteNonQuery();
                    }
                    else
                    {
                        transaction.Rollback();
                        return Results.BadRequest(new { message = "Mã giảm giá không tồn tại." });
                    }
                }

                discountTotal += comboDiscount;
                if (discountTotal > subTotal)
                {
                    discountTotal = subTotal;
                }
                var grandTotal = subTotal - discountTotal + deliveryFee;
                var paymentStatus = "Đang xử lý";

                using (var orderCmd = connection.CreateCommand())
                {
                    orderCmd.Transaction = transaction;
                    orderCmd.CommandText = """
                        INSERT INTO Orders (UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, PromoCode, Note, IdempotencyKey)
                        VALUES ($userId, $orderCode, 'Mới', $subTotal, $discountTotal, $deliveryFee, $grandTotal, $paymentStatus, $paymentMethod, $promoCode, $note, $idempotencyKey);
                        SELECT last_insert_rowid();
                        """;
                    orderCmd.Parameters.AddWithValue("$userId", request.UserId);
                    orderCmd.Parameters.AddWithValue("$orderCode", orderCode);
                    orderCmd.Parameters.AddWithValue("$subTotal", subTotal);
                    orderCmd.Parameters.AddWithValue("$discountTotal", discountTotal);
                    orderCmd.Parameters.AddWithValue("$deliveryFee", deliveryFee);
                    orderCmd.Parameters.AddWithValue("$grandTotal", grandTotal);
                    orderCmd.Parameters.AddWithValue("$paymentStatus", paymentStatus);
                    orderCmd.Parameters.AddWithValue("$paymentMethod", request.PaymentMethod.Trim());
                    orderCmd.Parameters.AddWithValue("$promoCode", (object?)promoCode ?? DBNull.Value);
                    orderCmd.Parameters.AddWithValue("$note", (object?)request.Note ?? DBNull.Value);
                    orderCmd.Parameters.AddWithValue("$idempotencyKey", (object?)idempotencyKey ?? DBNull.Value);
                    var orderId = (long)orderCmd.ExecuteScalar()!;

                    foreach (var item in items)
                    {
                        using var itemCmd = connection.CreateCommand();
                        itemCmd.Transaction = transaction;
                        itemCmd.CommandText = """
                            INSERT INTO OrderItem (OrderId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal)
                            VALUES ($orderId, $productId, $name, $unitPrice, $quantity, $lineTotal);
                            """;
                        itemCmd.Parameters.AddWithValue("$orderId", orderId);
                        itemCmd.Parameters.AddWithValue("$productId", item.ProductId);
                        itemCmd.Parameters.AddWithValue("$name", item.Name);
                        itemCmd.Parameters.AddWithValue("$unitPrice", item.Price);
                        itemCmd.Parameters.AddWithValue("$quantity", item.Quantity);
                        itemCmd.Parameters.AddWithValue("$lineTotal", item.LineTotal);
                        itemCmd.ExecuteNonQuery();

                        using var stockCmd = connection.CreateCommand();
                        stockCmd.Transaction = transaction;
                        stockCmd.CommandText = """
                            UPDATE Product
                            SET StockQty = StockQty - $quantity
                            WHERE ProductId = $productId;
                            """;
                        stockCmd.Parameters.AddWithValue("$quantity", item.Quantity);
                        stockCmd.Parameters.AddWithValue("$productId", item.ProductId);
                        stockCmd.ExecuteNonQuery();
                    }

                    using var historyCmd = connection.CreateCommand();
                    historyCmd.Transaction = transaction;
                    historyCmd.CommandText = """
                        INSERT INTO OrderStatusHistory (OrderId, FromStatus, ToStatus, ChangedByUserId, Note)
                        VALUES ($orderId, 'Mới', 'Mới', NULL, NULL);
                        """;
                    historyCmd.Parameters.AddWithValue("$orderId", orderId);
                    historyCmd.ExecuteNonQuery();

                    transaction.Commit();
                    return Results.Created($"/api/orders/{orderId}", new { OrderId = (int)orderId, OrderCode = orderCode });
                }
            }
            catch
            {
                transaction.Rollback();
                return Results.Problem("Failed to create order.");
            }
        });

        app.MapGet("/orders/{id:int}", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            var order = GetOrderById(connection, id);
            if (order is null)
            {
                return Results.NotFound();
            }

            var items = GetOrderItems(connection, id);
            return Results.Ok(new { Order = order, Items = items });
        });

        app.MapGet("/orders/{id:int}/history", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT HistoryId, OrderId, FromStatus, ToStatus, ChangedByUserId, ChangedAt, Note
                FROM OrderStatusHistory
                WHERE OrderId = $orderId
                ORDER BY ChangedAt ASC;
                """;
            cmd.Parameters.AddWithValue("$orderId", id);

            using var reader = cmd.ExecuteReader();
            var history = new List<object>();
            while (reader.Read())
            {
                history.Add(new
                {
                    HistoryId = reader.GetInt32(reader.GetOrdinal("HistoryId")),
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    FromStatus = reader.GetString(reader.GetOrdinal("FromStatus")),
                    ToStatus = reader.GetString(reader.GetOrdinal("ToStatus")),
                    ChangedByUserId = reader.IsDBNull(reader.GetOrdinal("ChangedByUserId"))
                        ? (int?)null
                        : reader.GetInt32(reader.GetOrdinal("ChangedByUserId")),
                    ChangedAt = reader.GetString(reader.GetOrdinal("ChangedAt")),
                    Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note"))
                });
            }

            return Results.Ok(history);
        });

        app.MapGet("/users/{userId:int}/orders", (int userId, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT OrderId, UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, Note, CreatedAt
                FROM Orders
                WHERE UserId = $userId
                ORDER BY CreatedAt DESC;
                """;
            cmd.Parameters.AddWithValue("$userId", userId);

            using var reader = cmd.ExecuteReader();
            var orders = new List<OrderResponse>();
            while (reader.Read())
            {
                orders.Add(ReadOrder(reader));
            }

            return Results.Ok(orders);
        });

        app.MapPut("/orders/{id:int}/status", (int id, OrderStatusRequest request, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return Results.BadRequest(new { message = "Status is required." });
            }

            var nextStatus = request.Status.Trim();
            if (!AllowedOrderStatuses.Contains(nextStatus))
            {
                return Results.BadRequest(new { message = "Invalid order status." });
            }

            using var connection = db.OpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                string? currentStatus;
                using (var selectCmd = connection.CreateCommand())
                {
                    selectCmd.Transaction = transaction;
                    selectCmd.CommandText = "SELECT Status FROM Orders WHERE OrderId = $id;";
                    selectCmd.Parameters.AddWithValue("$id", id);
                    currentStatus = selectCmd.ExecuteScalar() as string;
                }

                if (currentStatus is null)
                {
                    transaction.Rollback();
                    return Results.NotFound();
                }

                if (string.Equals(currentStatus, nextStatus, StringComparison.Ordinal))
                {
                    transaction.Commit();
                    return Results.NoContent();
                }

                using (var updateCmd = connection.CreateCommand())
                {
                    updateCmd.Transaction = transaction;
                    updateCmd.CommandText = "UPDATE Orders SET Status = $status WHERE OrderId = $id;";
                    updateCmd.Parameters.AddWithValue("$status", nextStatus);
                    updateCmd.Parameters.AddWithValue("$id", id);
                    updateCmd.ExecuteNonQuery();
                }

                using (var historyCmd = connection.CreateCommand())
                {
                    historyCmd.Transaction = transaction;
                    historyCmd.CommandText = """
                        INSERT INTO OrderStatusHistory (OrderId, FromStatus, ToStatus, ChangedByUserId, Note)
                        VALUES ($orderId, $fromStatus, $toStatus, $changedBy, $note);
                        """;
                    historyCmd.Parameters.AddWithValue("$orderId", id);
                    historyCmd.Parameters.AddWithValue("$fromStatus", currentStatus);
                    historyCmd.Parameters.AddWithValue("$toStatus", nextStatus);
                    historyCmd.Parameters.AddWithValue("$changedBy", (object?)request.ChangedByUserId ?? DBNull.Value);
                    historyCmd.Parameters.AddWithValue("$note", (object?)request.Note ?? DBNull.Value);
                    historyCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return Results.NoContent();
            }
            catch
            {
                transaction.Rollback();
                return Results.Problem("Failed to update order status.");
            }
        });

        app.MapPost("/orders/{id:int}/payment", (int id, PaymentRequest request, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                using (var orderCmd = connection.CreateCommand())
                {
                    orderCmd.Transaction = transaction;
                    orderCmd.CommandText = "SELECT 1 FROM Orders WHERE OrderId = $id;";
                    orderCmd.Parameters.AddWithValue("$id", id);
                    if (orderCmd.ExecuteScalar() is null)
                    {
                        transaction.Rollback();
                        return Results.NotFound();
                    }
                }

                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.Transaction = transaction;
                    insertCmd.CommandText = """
                        INSERT INTO Payment (OrderId, Provider, Amount, Status, TransactionRef, PaidAt)
                        VALUES ($orderId, $provider, $amount, $status, $transactionRef, datetime('now','localtime'));
                        SELECT last_insert_rowid();
                        """;
                    insertCmd.Parameters.AddWithValue("$orderId", id);
                    insertCmd.Parameters.AddWithValue("$provider", request.Provider.Trim());
                    insertCmd.Parameters.AddWithValue("$amount", request.Amount);
                    insertCmd.Parameters.AddWithValue("$status", request.Status.Trim());
                    insertCmd.Parameters.AddWithValue("$transactionRef", request.TransactionRef.Trim());
                    var paymentId = (long)insertCmd.ExecuteScalar()!;

                    using var updateCmd = connection.CreateCommand();
                    updateCmd.Transaction = transaction;
                    updateCmd.CommandText = """
                        UPDATE Orders
                        SET PaymentStatus = $status
                        WHERE OrderId = $orderId;
                        """;
                    updateCmd.Parameters.AddWithValue("$status", request.Status.Trim());
                    updateCmd.Parameters.AddWithValue("$orderId", id);
                    updateCmd.ExecuteNonQuery();

                    transaction.Commit();
                    return Results.Created($"/api/orders/{id}/payment", new { PaymentId = (int)paymentId });
                }
            }
            catch
            {
                transaction.Rollback();
                return Results.Problem("Failed to record payment.");
            }
        });

        return app;
    }

    private static OrderResponse? GetOrderById(SqliteConnection connection, int id)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT OrderId, UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, Note, CreatedAt
            FROM Orders
            WHERE OrderId = $id;
            """;
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return ReadOrder(reader);
    }

    private static List<OrderItemResponse> GetOrderItems(SqliteConnection connection, int orderId)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT OrderItemId, OrderId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal
            FROM OrderItem
            WHERE OrderId = $orderId;
            """;
        cmd.Parameters.AddWithValue("$orderId", orderId);

        using var reader = cmd.ExecuteReader();
        var items = new List<OrderItemResponse>();
        while (reader.Read())
        {
            items.Add(new OrderItemResponse(
                reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                reader.GetInt32(reader.GetOrdinal("OrderId")),
                reader.GetInt32(reader.GetOrdinal("ProductId")),
                reader.GetString(reader.GetOrdinal("ProductNameSnapshot")),
                reader.GetString(reader.GetOrdinal("UnitPriceSnapshot")),
                reader.GetInt32(reader.GetOrdinal("Quantity")),
                reader.GetDouble(reader.GetOrdinal("LineTotal"))
            ));
        }

        return items;
    }

    private static OrderResponse ReadOrder(SqliteDataReader reader)
    {
        return new OrderResponse(
            reader.GetInt32(reader.GetOrdinal("OrderId")),
            reader.GetInt32(reader.GetOrdinal("UserId")),
            reader.GetString(reader.GetOrdinal("OrderCode")),
            reader.GetString(reader.GetOrdinal("Status")),
            reader.GetDouble(reader.GetOrdinal("SubTotal")),
            reader.GetDouble(reader.GetOrdinal("DiscountTotal")),
            reader.GetDouble(reader.GetOrdinal("DeliveryFee")),
            reader.GetDouble(reader.GetOrdinal("GrandTotal")),
            reader.GetString(reader.GetOrdinal("PaymentStatus")),
            reader.GetString(reader.GetOrdinal("PaymentMethod")),
            reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note")),
            reader.GetString(reader.GetOrdinal("CreatedAt"))
        );
    }
}
