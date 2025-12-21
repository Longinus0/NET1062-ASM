using Backend.Data;

namespace Backend.Endpoints;

public static class PromoEndpoints
{
    public static IEndpointRouteBuilder MapPromoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/promo/validate", (string? code, Db db) =>
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest(new { message = "Promo code is required." });
            }

            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT Code, Type, Value, UsageLimit, UsedCount, ExpiresAt, IsActive
                FROM PromoCode
                WHERE Code = $code;
                """;
            cmd.Parameters.AddWithValue("$code", code.Trim().ToUpperInvariant());

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Results.NotFound(new { message = "Mã giảm giá không tồn tại." });
            }

            var isActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1;
            var expiresAt = reader.GetString(reader.GetOrdinal("ExpiresAt"));
            var usageLimit = reader.IsDBNull(reader.GetOrdinal("UsageLimit")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("UsageLimit"));
            var usedCount = reader.GetInt32(reader.GetOrdinal("UsedCount"));

            if (!isActive)
            {
                return Results.BadRequest(new { message = "Mã giảm giá đang tạm ngưng." });
            }

            if (DateTime.TryParse(expiresAt, out var expires) && expires < DateTime.Now)
            {
                return Results.BadRequest(new { message = "Mã giảm giá đã hết hạn." });
            }

            if (usageLimit.HasValue && usedCount >= usageLimit.Value)
            {
                return Results.BadRequest(new { message = "Mã giảm giá đã hết lượt sử dụng." });
            }

            var type = reader.GetString(reader.GetOrdinal("Type"));
            var value = reader.GetDouble(reader.GetOrdinal("Value"));

            return Results.Ok(new { Code = code.Trim().ToUpperInvariant(), Type = type, Value = value });
        });

        return app;
    }
}
