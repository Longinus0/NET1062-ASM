using Backend.Contracts;
using Backend.Data;

namespace Backend.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT UserId, FullName, Email, Phone, Address, AvatarUrl, IsActive, ForcePasswordReset, CreatedAt, UpdatedAt
                FROM Users
                WHERE UserId = $id;
                """;
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Results.NotFound();
            }

            var response = new UserResponse(
                reader.GetInt32(reader.GetOrdinal("UserId")),
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                reader.GetInt32(reader.GetOrdinal("IsActive")),
                reader.GetInt32(reader.GetOrdinal("ForcePasswordReset")),
                reader.GetString(reader.GetOrdinal("CreatedAt")),
                reader.GetString(reader.GetOrdinal("UpdatedAt"))
            );

            return Results.Ok(response);
        });

        app.MapPut("/users/{id:int}", (int id, UpdateProfileRequest request, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Results.BadRequest(new { message = "FullName is required." });
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Users
                SET FullName = $fullName,
                    Phone = $phone,
                    Address = $address,
                    AvatarUrl = $avatarUrl
                WHERE UserId = $id;
                """;
            cmd.Parameters.AddWithValue("$fullName", request.FullName.Trim());
            cmd.Parameters.AddWithValue("$phone", (object?)request.Phone?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$address", (object?)request.Address?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$avatarUrl", (object?)request.AvatarUrl?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        app.MapPut("/users/{id:int}/password", (int id, ChangePasswordRequest request, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Results.BadRequest(new { message = "CurrentPassword and NewPassword are required." });
            }

            using var connection = db.OpenConnection();
            using var readCmd = connection.CreateCommand();
            readCmd.CommandText = """
                SELECT PasswordHash
                FROM Users
                WHERE UserId = $id;
                """;
            readCmd.Parameters.AddWithValue("$id", id);
            var existingHash = readCmd.ExecuteScalar() as string;
            if (string.IsNullOrWhiteSpace(existingHash))
            {
                return Results.NotFound();
            }

            if (!string.Equals(existingHash, AuthEndpoints.HashPassword(request.CurrentPassword), StringComparison.Ordinal))
            {
                return Results.BadRequest(new { message = "Mật khẩu hiện tại không đúng." });
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Users
                SET PasswordHash = $passwordHash,
                    ForcePasswordReset = 0
                WHERE UserId = $id;
                """;
            cmd.Parameters.AddWithValue("$passwordHash", AuthEndpoints.HashPassword(request.NewPassword));
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        app.MapGet("/users/{id:int}/role", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT r.Name
                FROM UserRole ur
                JOIN Role r ON r.RoleId = ur.RoleId
                WHERE ur.UserId = $id;
                """;
            cmd.Parameters.AddWithValue("$id", id);

            var roleName = cmd.ExecuteScalar() as string;
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return Results.NotFound();
            }

            return Results.Ok(new UserRoleResponse(roleName));
        });

        return app;
    }
}
