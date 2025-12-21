namespace Backend.Contracts;

public sealed record UpdateProfileRequest(
    string FullName,
    string? Phone,
    string? Address,
    string? AvatarUrl
);

public sealed record AdminUpdateUserRequest(
    string FullName,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    int IsActive,
    int ForcePasswordReset
);

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    int IsActive,
    int ForcePasswordReset,
    int RoleId
);

public sealed record UserResponse(
    int UserId,
    string FullName,
    string Email,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    int IsActive,
    int ForcePasswordReset,
    string CreatedAt,
    string UpdatedAt
);

public sealed record UserRoleResponse(string RoleName);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AdminUserResponse(
    int UserId,
    string FullName,
    string Email,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    int IsActive,
    int ForcePasswordReset,
    string CreatedAt,
    string UpdatedAt,
    int? RoleId,
    string? RoleName
);

public sealed record AdminUserRoleRequest(int RoleId);
