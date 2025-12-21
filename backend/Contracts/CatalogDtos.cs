namespace Backend.Contracts;

public sealed record CategoryResponse(int CategoryId, string Name, string Description);

public sealed record CategoryRequest(string Name, string Description);

public sealed record ProductResponse(
    int ProductId,
    int CategoryId,
    string Name,
    string Description,
    double Price,
    string ImageUrl,
    string TopicTag,
    int IsAvailable,
    int StockQty,
    string CreatedAt,
    string UpdatedAt
);

public sealed record ProductRequest(
    int CategoryId,
    string Name,
    string Description,
    double Price,
    string ImageUrl,
    string TopicTag,
    int IsAvailable,
    int StockQty
);

public sealed record ComboResponse(int ComboId, string Name, string Description, double Price, string? ImageUrl, int IsActive);

public sealed record ComboRequest(string Name, string Description, double Price, string? ImageUrl, int IsActive);

public sealed record ComboItemResponse(int ProductId, string ProductName, int Quantity);
