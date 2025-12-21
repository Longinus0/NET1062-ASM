namespace Backend.Contracts;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string? Phone,
    string? Address,
    string? AvatarUrl
);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthUserResponse(
    int UserId,
    string FullName,
    string Email,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    int ForcePasswordReset
);

public sealed record ExternalLoginRequest(
    string Provider,
    int ProviderId,
    string Email,
    string FullName,
    string? Phone,
    string? Address,
    string? AvatarUrl
);

public sealed record GoogleOAuthRequest(string IdToken);

public sealed record FacebookOAuthRequest(string AccessToken);

public sealed record AdminLoginRequest(string Email, string Password);
