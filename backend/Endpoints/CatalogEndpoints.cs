using System.Text;
using Backend.Contracts;
using Backend.Data;
using Microsoft.Data.Sqlite;

namespace Backend.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", (Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT CategoryId, Name, Description FROM Category ORDER BY Name;";

            using var reader = cmd.ExecuteReader();
            var results = new List<CategoryResponse>();
            while (reader.Read())
            {
                results.Add(new CategoryResponse(
                    reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    reader.GetString(reader.GetOrdinal("Name")),
                    reader.GetString(reader.GetOrdinal("Description"))
                ));
            }

            return Results.Ok(results);
        });

        app.MapGet("/products", (Db db, string? search, string? description, double? minPrice, double? maxPrice, int? categoryId, string? topicTag, int? isAvailable) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();

            var sql = new StringBuilder("""
                SELECT ProductId, CategoryId, Name, Description, Price, ImageUrl, TopicTag, IsAvailable, StockQty, CreatedAt, UpdatedAt
                FROM Product
                WHERE 1 = 1
                """);

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql.Append(" AND Name LIKE $search");
                cmd.Parameters.AddWithValue("$search", $"%{search.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                sql.Append(" AND Description LIKE $description");
                cmd.Parameters.AddWithValue("$description", $"%{description.Trim()}%");
            }

            if (minPrice.HasValue)
            {
                sql.Append(" AND Price >= $minPrice");
                cmd.Parameters.AddWithValue("$minPrice", minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                sql.Append(" AND Price <= $maxPrice");
                cmd.Parameters.AddWithValue("$maxPrice", maxPrice.Value);
            }

            if (categoryId.HasValue)
            {
                sql.Append(" AND CategoryId = $categoryId");
                cmd.Parameters.AddWithValue("$categoryId", categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(topicTag))
            {
                sql.Append(" AND TopicTag = $topicTag");
                cmd.Parameters.AddWithValue("$topicTag", topicTag.Trim());
            }

            if (isAvailable.HasValue)
            {
                sql.Append(" AND IsAvailable = $isAvailable");
                cmd.Parameters.AddWithValue("$isAvailable", isAvailable.Value);
            }

            sql.Append(" ORDER BY CreatedAt DESC;");
            cmd.CommandText = sql.ToString();

            using var reader = cmd.ExecuteReader();
            var results = new List<ProductResponse>();
            while (reader.Read())
            {
                results.Add(ReadProduct(reader));
            }

            return Results.Ok(results);
        });

        app.MapGet("/products/{id:int}", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT ProductId, CategoryId, Name, Description, Price, ImageUrl, TopicTag, IsAvailable, StockQty, CreatedAt, UpdatedAt
                FROM Product
                WHERE ProductId = $id;
                """;
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Results.NotFound();
            }

            return Results.Ok(ReadProduct(reader));
        });

        app.MapGet("/combos", (Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ComboId, Name, Description, Price, ImageUrl, IsActive FROM Combo ORDER BY Name;";

            using var reader = cmd.ExecuteReader();
            var results = new List<ComboResponse>();
            while (reader.Read())
            {
                results.Add(new ComboResponse(
                    reader.GetInt32(reader.GetOrdinal("ComboId")),
                    reader.GetString(reader.GetOrdinal("Name")),
                    reader.GetString(reader.GetOrdinal("Description")),
                    reader.GetDouble(reader.GetOrdinal("Price")),
                    reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
                    reader.GetInt32(reader.GetOrdinal("IsActive"))
                ));
            }

            return Results.Ok(results);
        });

        app.MapGet("/combos/{id:int}", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ComboId, Name, Description, Price, ImageUrl, IsActive FROM Combo WHERE ComboId = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return Results.NotFound();
            }

            var combo = new ComboResponse(
                reader.GetInt32(reader.GetOrdinal("ComboId")),
                reader.GetString(reader.GetOrdinal("Name")),
                reader.GetString(reader.GetOrdinal("Description")),
                reader.GetDouble(reader.GetOrdinal("Price")),
                reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
                reader.GetInt32(reader.GetOrdinal("IsActive"))
            );

            return Results.Ok(combo);
        });

        app.MapGet("/combos/{id:int}/items", (int id, Db db) =>
        {
            using var connection = db.OpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT ci.ProductId, p.Name AS ProductName, ci.Quantity
                FROM ComboItem ci
                JOIN Product p ON p.ProductId = ci.ProductId
                WHERE ci.ComboId = $id;
                """;
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            var items = new List<ComboItemResponse>();
            while (reader.Read())
            {
                items.Add(new ComboItemResponse(
                    reader.GetInt32(reader.GetOrdinal("ProductId")),
                    reader.GetString(reader.GetOrdinal("ProductName")),
                    reader.GetInt32(reader.GetOrdinal("Quantity"))
                ));
            }

            return Results.Ok(items);
        });

        return app;
    }

    private static ProductResponse ReadProduct(SqliteDataReader reader)
    {
        return new ProductResponse(
            reader.GetInt32(reader.GetOrdinal("ProductId")),
            reader.GetInt32(reader.GetOrdinal("CategoryId")),
            reader.GetString(reader.GetOrdinal("Name")),
            reader.GetString(reader.GetOrdinal("Description")),
            reader.GetDouble(reader.GetOrdinal("Price")),
            reader.GetString(reader.GetOrdinal("ImageUrl")),
            reader.GetString(reader.GetOrdinal("TopicTag")),
            reader.GetInt32(reader.GetOrdinal("IsAvailable")),
            reader.GetInt32(reader.GetOrdinal("StockQty")),
            reader.GetString(reader.GetOrdinal("CreatedAt")),
            reader.GetString(reader.GetOrdinal("UpdatedAt"))
        );
    }
}
