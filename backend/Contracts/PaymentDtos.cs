namespace Backend.Contracts;

public sealed record PaymentRequest(
    string Provider,
    double Amount,
    string Status,
    string TransactionRef
);

public sealed record PaymentResponse(
    int PaymentId,
    int OrderId,
    string Provider,
    double Amount,
    string Status,
    string TransactionRef,
    string? PaidAt
);

public sealed record PaymentStatusRequest(string Status);
