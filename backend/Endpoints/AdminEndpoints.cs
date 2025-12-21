using Backend.Contracts;
using Backend.Data;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace Backend.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin");

        group.MapPost("/login", (AdminLoginRequest request, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and Password are required." });
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT u.UserId, u.FullName, u.Email, u.Phone, u.Address, u.AvatarUrl, u.PasswordHash, u.IsActive, u.ForcePasswordReset
                FROM Users u
                WHERE u.Email = $email;
                """;
            cmd.Parameters.AddWithValue("$email", request.Email.Trim());

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Results.Unauthorized();
            }

            var storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
            if (!string.Equals(storedHash, AuthEndpoints.HashPassword(request.Password), StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            var userId = reader.GetInt32(reader.GetOrdinal("UserId"));
            if (reader.GetInt32(reader.GetOrdinal("IsActive")) == 0)
            {
                return Results.BadRequest(new { message = "Tài khoản đã bị khóa." });
            }
            if (!IsAdmin(connection, userId))
            {
                return Results.Forbid();
            }

            var response = new AuthUserResponse(
                userId,
                reader.GetString(reader.GetOrdinal("FullName")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                reader.GetInt32(reader.GetOrdinal("ForcePasswordReset"))
            );

            return Results.Ok(response);
        });

        group.MapPost("/categories", (CategoryRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Category (Name, Description)
                VALUES ($name, $description);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$name", request.Name.Trim());
            cmd.Parameters.AddWithValue("$description", request.Description.Trim());
            var newId = (long)cmd.ExecuteScalar()!;

            return Results.Created($"/api/categories/{newId}", new { CategoryId = (int)newId });
        });

        group.MapPut("/categories/{id:int}", (int id, CategoryRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Category
                SET Name = $name, Description = $description
                WHERE CategoryId = $id;
                """;
            cmd.Parameters.AddWithValue("$name", request.Name.Trim());
            cmd.Parameters.AddWithValue("$description", request.Description.Trim());
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapDelete("/categories/{id:int}", (int id, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Category WHERE CategoryId = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapPost("/products", (ProductRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Product (CategoryId, Name, Description, Price, ImageUrl, TopicTag, IsAvailable, StockQty)
                VALUES ($categoryId, $name, $description, $price, $imageUrl, $topicTag, $isAvailable, $stockQty);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$categoryId", request.CategoryId);
            cmd.Parameters.AddWithValue("$name", request.Name.Trim());
            cmd.Parameters.AddWithValue("$description", request.Description.Trim());
            cmd.Parameters.AddWithValue("$price", request.Price);
            cmd.Parameters.AddWithValue("$imageUrl", request.ImageUrl.Trim());
            cmd.Parameters.AddWithValue("$topicTag", request.TopicTag.Trim());
            cmd.Parameters.AddWithValue("$isAvailable", request.IsAvailable);
            cmd.Parameters.AddWithValue("$stockQty", request.StockQty);
            var newId = (long)cmd.ExecuteScalar()!;

            return Results.Created($"/api/products/{newId}", new { ProductId = (int)newId });
        });

        group.MapPut("/products/{id:int}", (int id, ProductRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Product
                SET CategoryId = $categoryId,
                    Name = $name,
                    Description = $description,
                    Price = $price,
                    ImageUrl = $imageUrl,
                    TopicTag = $topicTag,
                    IsAvailable = $isAvailable,
                    StockQty = $stockQty
                WHERE ProductId = $id;
                """;
            cmd.Parameters.AddWithValue("$categoryId", request.CategoryId);
            cmd.Parameters.AddWithValue("$name", request.Name.Trim());
            cmd.Parameters.AddWithValue("$description", request.Description.Trim());
            cmd.Parameters.AddWithValue("$price", request.Price);
            cmd.Parameters.AddWithValue("$imageUrl", request.ImageUrl.Trim());
            cmd.Parameters.AddWithValue("$topicTag", request.TopicTag.Trim());
            cmd.Parameters.AddWithValue("$isAvailable", request.IsAvailable);
            cmd.Parameters.AddWithValue("$stockQty", request.StockQty);
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapDelete("/products/{id:int}", (int id, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Product WHERE ProductId = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapPost("/combos", (ComboRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Combo (Name, Description, Price, ImageUrl, IsActive)
                VALUES ($name, $description, $price, $imageUrl, $isActive);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$name", request.Name.Trim());
            cmd.Parameters.AddWithValue("$description", request.Description.Trim());
            cmd.Parameters.AddWithValue("$price", request.Price);
            cmd.Parameters.AddWithValue("$imageUrl", (object?)request.ImageUrl?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$isActive", request.IsActive);
            var newId = (long)cmd.ExecuteScalar()!;

            return Results.Created($"/api/combos/{newId}", new { ComboId = (int)newId });
        });

        group.MapPut("/combos/{id:int}", (int id, ComboRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Combo
                SET Name = $name,
                    Description = $description,
                    Price = $price,
                    ImageUrl = $imageUrl,
                    IsActive = $isActive
                WHERE ComboId = $id;
                """;
            cmd.Parameters.AddWithValue("$name", request.Name.Trim());
            cmd.Parameters.AddWithValue("$description", request.Description.Trim());
            cmd.Parameters.AddWithValue("$price", request.Price);
            cmd.Parameters.AddWithValue("$imageUrl", (object?)request.ImageUrl?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$isActive", request.IsActive);
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapDelete("/combos/{id:int}", (int id, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Combo WHERE ComboId = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapGet("/users", (Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT u.UserId, u.FullName, u.Email, u.Phone, u.Address, u.AvatarUrl, u.IsActive, u.ForcePasswordReset, u.CreatedAt, u.UpdatedAt,
                       r.RoleId, r.Name AS RoleName
                FROM Users u
                LEFT JOIN UserRole ur ON ur.UserId = u.UserId
                LEFT JOIN Role r ON r.RoleId = ur.RoleId
                ORDER BY u.CreatedAt DESC;
                """;

            using var reader = cmd.ExecuteReader();
            var users = new List<AdminUserResponse>();
            while (reader.Read())
            {
                users.Add(new AdminUserResponse(
                    reader.GetInt32(reader.GetOrdinal("UserId")),
                    reader.GetString(reader.GetOrdinal("FullName")),
                    reader.GetString(reader.GetOrdinal("Email")),
                    reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                    reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                    reader.GetInt32(reader.GetOrdinal("IsActive")),
                    reader.GetInt32(reader.GetOrdinal("ForcePasswordReset")),
                    reader.GetString(reader.GetOrdinal("CreatedAt")),
                    reader.GetString(reader.GetOrdinal("UpdatedAt")),
                    reader.IsDBNull(reader.GetOrdinal("RoleId")) ? null : reader.GetInt32(reader.GetOrdinal("RoleId")),
                    reader.IsDBNull(reader.GetOrdinal("RoleName")) ? null : reader.GetString(reader.GetOrdinal("RoleName"))
                ));
            }

            return Results.Ok(users);
        });

        group.MapGet("/roles", (Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT r.RoleId, r.Name, COUNT(ur.UserId) AS UserCount
                FROM Role r
                LEFT JOIN UserRole ur ON ur.RoleId = r.RoleId
                GROUP BY r.RoleId, r.Name
                ORDER BY r.RoleId;
                """;

            using var reader = cmd.ExecuteReader();
            var roles = new List<RoleResponse>();
            while (reader.Read())
            {
                var roleId = reader.GetInt32(reader.GetOrdinal("RoleId"));
                var roleName = reader.GetString(reader.GetOrdinal("Name"));
                var userCount = reader.GetInt32(reader.GetOrdinal("UserCount"));

                using var userCmd = connection.CreateCommand();
                userCmd.CommandText = """
                    SELECT u.FullName, u.Email
                    FROM UserRole ur
                    JOIN Users u ON u.UserId = ur.UserId
                    WHERE ur.RoleId = $roleId
                    ORDER BY u.FullName
                    LIMIT 3;
                    """;
                userCmd.Parameters.AddWithValue("$roleId", roleId);
                using var userReader = userCmd.ExecuteReader();
                var users = new List<RoleUserPreview>();
                while (userReader.Read())
                {
                    users.Add(new RoleUserPreview(
                        userReader.GetString(userReader.GetOrdinal("FullName")),
                        userReader.GetString(userReader.GetOrdinal("Email"))
                    ));
                }

                roles.Add(new RoleResponse(roleId, roleName, userCount, users));
            }

            return Results.Ok(roles);
        });

        group.MapPost("/users", (CreateUserRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();

            using (var checkCmd = connection.CreateCommand())
            {
                checkCmd.CommandText = "SELECT 1 FROM Users WHERE Email = $email;";
                checkCmd.Parameters.AddWithValue("$email", request.Email.Trim());
                if (checkCmd.ExecuteScalar() is not null)
                {
                    return Results.Conflict(new { message = "Email already exists." });
                }
            }

            using (var roleCmd = connection.CreateCommand())
            {
                roleCmd.CommandText = "SELECT 1 FROM Role WHERE RoleId = $roleId;";
                roleCmd.Parameters.AddWithValue("$roleId", request.RoleId);
                if (roleCmd.ExecuteScalar() is null)
                {
                    return Results.BadRequest(new { message = "Role không tồn tại." });
                }
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Users (FullName, Email, Phone, PasswordHash, Address, AvatarUrl, IsActive, ForcePasswordReset)
                VALUES ($fullName, $email, $phone, $passwordHash, $address, $avatarUrl, $isActive, $forcePasswordReset);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("$fullName", request.FullName.Trim());
            cmd.Parameters.AddWithValue("$email", request.Email.Trim());
            cmd.Parameters.AddWithValue("$phone", (object?)request.Phone?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$passwordHash", AuthEndpoints.HashPassword(request.Password));
            cmd.Parameters.AddWithValue("$address", (object?)request.Address?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$avatarUrl", (object?)request.AvatarUrl?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$isActive", request.IsActive);
            cmd.Parameters.AddWithValue("$forcePasswordReset", request.ForcePasswordReset);
            var newId = (long)cmd.ExecuteScalar()!;

            using (var roleInsertCmd = connection.CreateCommand())
            {
                roleInsertCmd.CommandText = """
                    INSERT INTO UserRole (UserId, RoleId)
                    VALUES ($userId, $roleId);
                    """;
                roleInsertCmd.Parameters.AddWithValue("$userId", (int)newId);
                roleInsertCmd.Parameters.AddWithValue("$roleId", request.RoleId);
                roleInsertCmd.ExecuteNonQuery();
            }

            var adminUserId = GetAdminUserId(httpRequest);
            InsertAuditLog(
                connection,
                adminUserId,
                "CREATE",
                "Users",
                (int)newId,
                "{}",
                JsonSerializer.Serialize(new
                {
                    request.FullName,
                    request.Email,
                    request.Phone,
                    request.Address,
                    request.AvatarUrl,
                    request.IsActive,
                    request.ForcePasswordReset,
                    request.RoleId
                }),
                httpRequest
            );

            return Results.Created($"/api/users/{newId}", new { UserId = (int)newId });
        });

        group.MapPut("/users/{id:int}", (int id, AdminUpdateUserRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            UserResponse? existing = null;
            using (var readCmd = connection.CreateCommand())
            {
                readCmd.CommandText = """
                    SELECT UserId, FullName, Email, Phone, Address, AvatarUrl, IsActive, ForcePasswordReset, CreatedAt, UpdatedAt
                    FROM Users
                    WHERE UserId = $id;
                    """;
                readCmd.Parameters.AddWithValue("$id", id);
                using var reader = readCmd.ExecuteReader();
                if (reader.Read())
                {
                    existing = new UserResponse(
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
                }
            }

            if (existing is null)
            {
                return Results.NotFound();
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Users
                SET FullName = $fullName,
                    Phone = $phone,
                    Address = $address,
                    AvatarUrl = $avatarUrl,
                    IsActive = $isActive,
                    ForcePasswordReset = $forcePasswordReset
                WHERE UserId = $id;
                """;
            cmd.Parameters.AddWithValue("$fullName", request.FullName.Trim());
            cmd.Parameters.AddWithValue("$phone", (object?)request.Phone?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$address", (object?)request.Address?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$avatarUrl", (object?)request.AvatarUrl?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$isActive", request.IsActive);
            cmd.Parameters.AddWithValue("$forcePasswordReset", request.ForcePasswordReset);
            cmd.Parameters.AddWithValue("$id", id);

            if (cmd.ExecuteNonQuery() == 0)
            {
                return Results.NotFound();
            }

            var adminUserId = GetAdminUserId(httpRequest);
            InsertAuditLog(
                connection,
                adminUserId,
                "UPDATE",
                "Users",
                id,
                JsonSerializer.Serialize(existing),
                JsonSerializer.Serialize(new
                {
                    request.FullName,
                    existing.Email,
                    request.Phone,
                    request.Address,
                    request.AvatarUrl,
                    request.IsActive,
                    request.ForcePasswordReset
                }),
                httpRequest
            );

            return Results.NoContent();
        });

        group.MapDelete("/users/{id:int}", (int id, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            var adminUserId = GetAdminUserId(httpRequest);
            if (adminUserId == id)
            {
                return Results.BadRequest(new { message = "Không thể khóa quản trị viên hiện tại." });
            }

            if (IsAdmin(connection, id))
            {
                return Results.BadRequest(new { message = "Không thể khóa tài khoản quản trị." });
            }

            string oldValuesJson = "{}";
            using (var readCmd = connection.CreateCommand())
            {
                readCmd.CommandText = """
                    SELECT UserId, FullName, Email, Phone, Address, AvatarUrl, IsActive, ForcePasswordReset, CreatedAt, UpdatedAt
                    FROM Users
                    WHERE UserId = $id;
                    """;
                readCmd.Parameters.AddWithValue("$id", id);
                using var reader = readCmd.ExecuteReader();
                if (!reader.Read())
                {
                    return Results.NotFound();
                }
                oldValuesJson = JsonSerializer.Serialize(new
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                    AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                    IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")),
                    ForcePasswordReset = reader.GetInt32(reader.GetOrdinal("ForcePasswordReset"))
                });
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                UPDATE Users
                SET IsActive = 0
                WHERE UserId = $id;
                """;
            cmd.Parameters.AddWithValue("$id", id);

            if (cmd.ExecuteNonQuery() == 0)
            {
                return Results.NotFound();
            }

            InsertAuditLog(
                connection,
                adminUserId,
                "STATUS_CHANGE",
                "Users",
                id,
                oldValuesJson,
                JsonSerializer.Serialize(new { IsActive = 0 }),
                httpRequest
            );

            return Results.NoContent();
        });

        group.MapGet("/orders", (Db db, HttpRequest httpRequest, string? status) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            var sql = """
                SELECT OrderId, UserId, OrderCode, Status, SubTotal, DiscountTotal, DeliveryFee, GrandTotal, PaymentStatus, PaymentMethod, Note, CreatedAt
                FROM Orders
                WHERE 1 = 1
                """;
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND Status = $status";
                cmd.Parameters.AddWithValue("$status", status.Trim());
            }

            sql += " ORDER BY CreatedAt DESC;";
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            var orders = new List<OrderResponse>();
            while (reader.Read())
            {
                orders.Add(new OrderResponse(
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
                ));
            }

            return Results.Ok(orders);
        });

        group.MapPut("/orders/{id:int}/payment-status", (int id, PaymentStatusRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Orders SET PaymentStatus = $status WHERE OrderId = $id;";
            cmd.Parameters.AddWithValue("$status", request.Status);
            cmd.Parameters.AddWithValue("$id", id);

            return cmd.ExecuteNonQuery() == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapPut("/users/{id:int}/role", (int id, AdminUserRoleRequest request, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            using var connection = db.OpenConnection();

            using (var roleCmd = connection.CreateCommand())
            {
                roleCmd.CommandText = "SELECT 1 FROM Role WHERE RoleId = $roleId;";
                roleCmd.Parameters.AddWithValue("$roleId", request.RoleId);
                if (roleCmd.ExecuteScalar() is null)
                {
                    return Results.BadRequest(new { message = "Role không tồn tại." });
                }
            }

            int? oldRoleId = null;
            using (var currentCmd = connection.CreateCommand())
            {
                currentCmd.CommandText = "SELECT RoleId FROM UserRole WHERE UserId = $userId;";
                currentCmd.Parameters.AddWithValue("$userId", id);
                if (currentCmd.ExecuteScalar() is int roleId)
                {
                    oldRoleId = roleId;
                }
            }

            using (var deleteCmd = connection.CreateCommand())
            {
                deleteCmd.CommandText = "DELETE FROM UserRole WHERE UserId = $userId;";
                deleteCmd.Parameters.AddWithValue("$userId", id);
                deleteCmd.ExecuteNonQuery();
            }

            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO UserRole (UserId, RoleId) VALUES ($userId, $roleId);";
                insertCmd.Parameters.AddWithValue("$userId", id);
                insertCmd.Parameters.AddWithValue("$roleId", request.RoleId);
                insertCmd.ExecuteNonQuery();
            }

            var adminUserId = GetAdminUserId(httpRequest);
            InsertAuditLog(
                connection,
                adminUserId,
                "UPDATE",
                "UserRole",
                id,
                JsonSerializer.Serialize(new { RoleId = oldRoleId }),
                JsonSerializer.Serialize(new { RoleId = request.RoleId }),
                httpRequest
            );

            return Results.NoContent();
        });

        group.MapGet("/revenue", (int? days, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            var windowDays = Math.Clamp(days ?? 30, 1, 365);

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT date(CreatedAt) AS Day, SUM(GrandTotal) AS Total
                FROM Orders
                WHERE CreatedAt >= datetime('now', 'localtime', $window)
                  AND PaymentStatus = 'Đã thanh toán'
                GROUP BY date(CreatedAt)
                ORDER BY date(CreatedAt);
                """;
            cmd.Parameters.AddWithValue("$window", $"-{windowDays} days");

            using var reader = cmd.ExecuteReader();
            var series = new List<object>();
            double total = 0;
            while (reader.Read())
            {
                var day = reader.GetString(reader.GetOrdinal("Day"));
                var amount = reader.GetDouble(reader.GetOrdinal("Total"));
                total += amount;
                series.Add(new { Date = day, Total = amount });
            }

            return Results.Ok(new { Total = total, Days = windowDays, Series = series });
        });

        group.MapGet("/audit-logs", (int? limit, Db db, HttpRequest httpRequest) =>
        {
            var authResult = RequireAdmin(httpRequest, db);
            if (authResult is not null)
            {
                return authResult;
            }

            var rowLimit = Math.Clamp(limit ?? 200, 1, 500);

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT a.AuditLogId, a.ActorUserid, u.FullName, u.Email, a.Action, a.EntityName, a.EntityId,
                       a.OldValuesJson, a.NewValuesJson, a.CreatedAt, a.IpAddress
                FROM AuditLog a
                LEFT JOIN Users u ON u.UserId = a.ActorUserid
                ORDER BY a.CreatedAt DESC
                LIMIT $limit;
                """;
            cmd.Parameters.AddWithValue("$limit", rowLimit);

            using var reader = cmd.ExecuteReader();
            var logs = new List<AuditLogResponse>();
            while (reader.Read())
            {
                logs.Add(new AuditLogResponse(
                    reader.GetInt32(reader.GetOrdinal("AuditLogId")),
                    reader.GetInt32(reader.GetOrdinal("ActorUserid")),
                    reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                    reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    reader.GetString(reader.GetOrdinal("Action")),
                    reader.GetString(reader.GetOrdinal("EntityName")),
                    reader.GetInt32(reader.GetOrdinal("EntityId")),
                    reader.GetString(reader.GetOrdinal("OldValuesJson")),
                    reader.GetString(reader.GetOrdinal("NewValuesJson")),
                    reader.GetString(reader.GetOrdinal("CreatedAt")),
                    reader.GetString(reader.GetOrdinal("IpAddress"))
                ));
            }

            return Results.Ok(logs);
        });

        return app;
    }

    private static IResult? RequireAdmin(HttpRequest request, Db db)
    {
        if (!request.Headers.TryGetValue("X-Admin-UserId", out var values) ||
            !int.TryParse(values.FirstOrDefault(), out var adminUserId))
        {
            return Results.Unauthorized();
        }

        using var connection = db.OpenConnection();
        return IsAdmin(connection, adminUserId) ? null : Results.Forbid();
    }

    private static bool IsAdmin(SqliteConnection connection, int userId)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT 1
            FROM UserRole ur
            JOIN Role r ON r.RoleId = ur.RoleId
            WHERE ur.UserId = $userId AND (r.Name = 'Admin' OR r.Name = 'Quản trị' OR r.Name = 'Quan tri');
            """;
        cmd.Parameters.AddWithValue("$userId", userId);
        return cmd.ExecuteScalar() is not null;
    }

    private static int GetAdminUserId(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("X-Admin-UserId", out var values) ||
            !int.TryParse(values.FirstOrDefault(), out var adminUserId))
        {
            return 0;
        }

        return adminUserId;
    }

    private static void InsertAuditLog(
        SqliteConnection connection,
        int actorUserId,
        string action,
        string entityName,
        int entityId,
        string oldValuesJson,
        string newValuesJson,
        HttpRequest request
    )
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO AuditLog (ActorUserid, Action, EntityName, EntityId, OldValuesJson, NewValuesJson, IpAddress)
            VALUES ($actorUserId, $action, $entityName, $entityId, $oldValuesJson, $newValuesJson, $ipAddress);
            """;
        cmd.Parameters.AddWithValue("$actorUserId", actorUserId);
        cmd.Parameters.AddWithValue("$action", action);
        cmd.Parameters.AddWithValue("$entityName", entityName);
        cmd.Parameters.AddWithValue("$entityId", entityId);
        cmd.Parameters.AddWithValue("$oldValuesJson", oldValuesJson);
        cmd.Parameters.AddWithValue("$newValuesJson", newValuesJson);
        cmd.Parameters.AddWithValue("$ipAddress", request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        cmd.ExecuteNonQuery();
    }
}
