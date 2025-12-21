namespace Backend.Contracts;

public record AuditLogResponse(
    int AuditLogId,
    int ActorUserId,
    string? ActorName,
    string? ActorEmail,
    string Action,
    string EntityName,
    int EntityId,
    string OldValuesJson,
    string NewValuesJson,
    string CreatedAt,
    string IpAddress
);
