namespace Backend.Contracts;

public sealed record OrderItemRequest(int ProductId, int Quantity);

public sealed record OrderCreateRequest(
    int UserId,
    string PaymentMethod,
    string? PromoCode,
    double? ComboDiscount,
    string? Note,
    List<OrderItemRequest> Items
);

public sealed record OrderResponse(
    int OrderId,
    int UserId,
    string OrderCode,
    string Status,
    double SubTotal,
    double DiscountTotal,
    double DeliveryFee,
    double GrandTotal,
    string PaymentStatus,
    string PaymentMethod,
    string? Note,
    string CreatedAt
);

public sealed record OrderItemResponse(
    int OrderItemId,
    int OrderId,
    int ProductId,
    string ProductNameSnapshot,
    string UnitPriceSnapshot,
    int Quantity,
    double LineTotal
);

public sealed record OrderStatusRequest(string Status, int? ChangedByUserId, string? Note);
