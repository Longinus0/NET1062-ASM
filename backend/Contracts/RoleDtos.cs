namespace Backend.Contracts;

public sealed record RoleUserPreview(string FullName, string Email);

public sealed record RoleResponse(int RoleId, string Name, int UserCount, List<RoleUserPreview> Users);
