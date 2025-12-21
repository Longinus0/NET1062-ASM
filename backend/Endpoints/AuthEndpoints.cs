using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Backend.Contracts;
using Backend.Data;

namespace Backend.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", (RegisterRequest request, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "FullName, Email, and Password are required." });
            }

            using var connection = db.OpenConnection();

            using (var checkCmd = connection.CreateCommand())
            {
                checkCmd.CommandText = "SELECT 1 FROM Users WHERE Email = $email;";
                checkCmd.Parameters.AddWithValue("$email", request.Email.Trim());
                if (checkCmd.ExecuteScalar() != null)
                {
                    return Results.Conflict(new { message = "Email already exists." });
                }
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Users (FullName, Email, Phone, PasswordHash, Address, AvatarUrl, IsActive, ForcePasswordReset)
                VALUES ($fullName, $email, $phone, $passwordHash, $address, $avatarUrl, 1, 0);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$fullName", request.FullName.Trim());
            cmd.Parameters.AddWithValue("$email", request.Email.Trim());
            cmd.Parameters.AddWithValue("$phone", (object?)request.Phone?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$passwordHash", HashPassword(request.Password));
            cmd.Parameters.AddWithValue("$address", (object?)request.Address?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$avatarUrl", (object?)request.AvatarUrl?.Trim() ?? DBNull.Value);

            var newId = (long)cmd.ExecuteScalar()!;

            var response = new AuthUserResponse(
                (int)newId,
                request.FullName.Trim(),
                request.Email.Trim(),
                request.Phone?.Trim(),
                request.Address?.Trim(),
                request.AvatarUrl?.Trim(),
                0
            );

            return Results.Created($"/api/users/{newId}", response);
        });

        group.MapPost("/login", (LoginRequest request, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and Password are required." });
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT UserId, FullName, Email, Phone, Address, AvatarUrl, PasswordHash, IsActive, ForcePasswordReset
                FROM Users
                WHERE Email = $email;
                """;
            cmd.Parameters.AddWithValue("$email", request.Email.Trim());

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Results.Unauthorized();
            }

            var storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
            if (!string.Equals(storedHash, HashPassword(request.Password), StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            if (reader.GetInt32(reader.GetOrdinal("IsActive")) == 0)
            {
                return Results.BadRequest(new { message = "Tài khoản đã bị khóa." });
            }

            var response = new AuthUserResponse(
                reader.GetInt32(reader.GetOrdinal("UserId")),
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                reader.GetInt32(reader.GetOrdinal("ForcePasswordReset"))
            );

            return Results.Ok(response);
        });

        group.MapPost("/external-login", (ExternalLoginRequest request, Db db) =>
        {
            return HandleExternalLogin(request, db);
        });

        group.MapPost("/oauth/google", async (GoogleOAuthRequest request, Db db, IConfiguration config, IHttpClientFactory httpClientFactory) =>
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return Results.BadRequest(new { message = "IdToken is required." });
            }

            using var client = httpClientFactory.CreateClient();
            var tokenInfoUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(request.IdToken)}";
            using var tokenResponse = await client.GetAsync(tokenInfoUrl);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return Results.Unauthorized();
            }

            using var tokenDoc = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
            var tokenRoot = tokenDoc.RootElement;
            var email = tokenRoot.GetProperty("email").GetString();
            var fullName = tokenRoot.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            var avatar = tokenRoot.TryGetProperty("picture", out var picProp) ? picProp.GetString() : null;
            var aud = tokenRoot.TryGetProperty("aud", out var audProp) ? audProp.GetString() : null;
            var emailVerified = tokenRoot.TryGetProperty("email_verified", out var verifiedProp) ? verifiedProp.GetString() : null;

            var googleClientId = config["OAuth:Google:ClientId"];
            if (!string.IsNullOrWhiteSpace(googleClientId) && !string.Equals(aud, googleClientId, StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            if (string.Equals(emailVerified, "false", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName))
            {
                return Results.BadRequest(new { message = "Email and name are required." });
            }

            var externalRequest = new ExternalLoginRequest(
                "Google",
                1,
                email,
                fullName,
                null,
                null,
                avatar
            );

            return HandleExternalLogin(externalRequest, db);
        });

        group.MapPost("/oauth/facebook", async (FacebookOAuthRequest request, Db db, IConfiguration config, IHttpClientFactory httpClientFactory) =>
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
            {
                return Results.BadRequest(new { message = "AccessToken is required." });
            }

            using var client = httpClientFactory.CreateClient();
            var appId = config["OAuth:Facebook:AppId"];
            var appSecret = config["OAuth:Facebook:AppSecret"];
            if (!string.IsNullOrWhiteSpace(appId) && !string.IsNullOrWhiteSpace(appSecret))
            {
                var appAccessToken = $"{appId}|{appSecret}";
                var debugUrl =
                    $"https://graph.facebook.com/debug_token?input_token={Uri.EscapeDataString(request.AccessToken)}&access_token={Uri.EscapeDataString(appAccessToken)}";
                using var debugResponse = await client.GetAsync(debugUrl);
                if (!debugResponse.IsSuccessStatusCode)
                {
                    return Results.Unauthorized();
                }

                using var debugDoc = JsonDocument.Parse(await debugResponse.Content.ReadAsStringAsync());
                if (!debugDoc.RootElement.TryGetProperty("data", out var debugData))
                {
                    return Results.Unauthorized();
                }

                var isValid = debugData.TryGetProperty("is_valid", out var validProp) && validProp.GetBoolean();
                var debugAppId = debugData.TryGetProperty("app_id", out var appProp) ? appProp.GetString() : null;
                if (!isValid || (!string.IsNullOrWhiteSpace(appId) && !string.Equals(appId, debugAppId, StringComparison.Ordinal)))
                {
                    return Results.Unauthorized();
                }
            }

            var userUrl =
                $"https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token={Uri.EscapeDataString(request.AccessToken)}";
            using var userResponse = await client.GetAsync(userUrl);
            if (!userResponse.IsSuccessStatusCode)
            {
                return Results.Unauthorized();
            }

            using var userDoc = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());
            var userRoot = userDoc.RootElement;
            var email = userRoot.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var fullName = userRoot.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            string? avatar = null;
            if (userRoot.TryGetProperty("picture", out var picRoot) &&
                picRoot.TryGetProperty("data", out var picData) &&
                picData.TryGetProperty("url", out var urlProp))
            {
                avatar = urlProp.GetString();
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName))
            {
                return Results.BadRequest(new { message = "Email and name are required." });
            }

            var externalRequest = new ExternalLoginRequest(
                "Facebook",
                2,
                email,
                fullName,
                null,
                null,
                avatar
            );

            return HandleExternalLogin(externalRequest, db);
        });

        return app;
    }

    private static IResult HandleExternalLogin(ExternalLoginRequest request, Db db)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FullName))
        {
            return Results.BadRequest(new { message = "Provider, Email, and FullName are required." });
        }

        using var connection = db.OpenConnection();

        using (var existingCmd = connection.CreateCommand())
        {
            existingCmd.CommandText = """
                SELECT u.UserId, u.FullName, u.Email, u.Phone, u.Address, u.AvatarUrl, u.IsActive
                FROM ExternalLogin e
                JOIN Users u ON u.UserId = e.UserId
                WHERE e.Provider = $provider AND e.Email = $email;
                """;
            existingCmd.Parameters.AddWithValue("$provider", request.Provider.Trim());
            existingCmd.Parameters.AddWithValue("$email", request.Email.Trim());

            using var reader = existingCmd.ExecuteReader();
            if (reader.Read())
            {
                if (reader.GetInt32(reader.GetOrdinal("IsActive")) == 0)
                {
                    return Results.BadRequest(new { message = "Tài khoản đã bị khóa." });
                }

                return Results.Ok(new AuthUserResponse(
                    reader.GetInt32(reader.GetOrdinal("UserId")),
                    reader.GetString(reader.GetOrdinal("FullName")),
                    reader.GetString(reader.GetOrdinal("Email")),
                    reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                    reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                    0
                ));
            }
        }

        int userId;
        using (var userCmd = connection.CreateCommand())
        {
            userCmd.CommandText = """
                SELECT UserId, FullName, Email, Phone, Address, AvatarUrl, IsActive
                FROM Users
                WHERE Email = $email;
                """;
            userCmd.Parameters.AddWithValue("$email", request.Email.Trim());

            using var reader = userCmd.ExecuteReader();
            if (reader.Read())
            {
                if (reader.GetInt32(reader.GetOrdinal("IsActive")) == 0)
                {
                    return Results.BadRequest(new { message = "Tài khoản đã bị khóa." });
                }

                userId = reader.GetInt32(reader.GetOrdinal("UserId"));
            }
            else
            {
                reader.Close();
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = """
                    INSERT INTO Users (FullName, Email, Phone, PasswordHash, Address, AvatarUrl, IsActive, ForcePasswordReset)
                    VALUES ($fullName, $email, $phone, $passwordHash, $address, $avatarUrl, 1, 0);
                    SELECT last_insert_rowid();
                    """;
                insertCmd.Parameters.AddWithValue("$fullName", request.FullName.Trim());
                insertCmd.Parameters.AddWithValue("$email", request.Email.Trim());
                insertCmd.Parameters.AddWithValue("$phone", (object?)request.Phone?.Trim() ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("$passwordHash", HashPassword($"external:{Guid.NewGuid()}"));
                insertCmd.Parameters.AddWithValue("$address", (object?)request.Address?.Trim() ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("$avatarUrl", (object?)request.AvatarUrl?.Trim() ?? DBNull.Value);
                userId = (int)(long)insertCmd.ExecuteScalar()!;
            }
        }

        using (var insertExternal = connection.CreateCommand())
        {
            insertExternal.CommandText = """
                INSERT INTO ExternalLogin (UserId, Provider, ProviderId, Email, CreatedAt)
                VALUES ($userId, $provider, $providerId, $email, datetime('now','localtime'));
                """;
            insertExternal.Parameters.AddWithValue("$userId", userId);
            insertExternal.Parameters.AddWithValue("$provider", request.Provider.Trim());
            insertExternal.Parameters.AddWithValue("$providerId", request.ProviderId);
            insertExternal.Parameters.AddWithValue("$email", request.Email.Trim());
            insertExternal.ExecuteNonQuery();
        }

        return Results.Ok(new AuthUserResponse(
            userId,
            request.FullName.Trim(),
            request.Email.Trim(),
            request.Phone?.Trim(),
            request.Address?.Trim(),
            request.AvatarUrl?.Trim(),
            0
        ));
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
