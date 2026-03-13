using Npgsql;
using NpgsqlTypes;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Infrastructure.Database.Providers;

public class PostgreSqlDatabaseProvider : IDatabaseProvider
{
    public DatabaseProviderType ProviderType => DatabaseProviderType.PostgreSql;

    public async Task TestConnectionAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1", conn);
        await cmd.ExecuteScalarAsync();
    }

    public async Task<List<string>> GetTableNamesAsync(string connectionString)
    {
        var tables = new List<string>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(@"
            SELECT table_schema || '.' || table_name 
            FROM information_schema.tables 
            WHERE table_schema NOT IN ('pg_catalog', 'information_schema') 
              AND table_type = 'BASE TABLE'
            ORDER BY table_schema, table_name", conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tables.Add(reader.GetString(0));

        return tables;
    }

    public async Task<TableSchema> GetTableSchemaAsync(string connectionString, string tableName)
    {
        var parts = tableName.Contains('.') ? tableName.Split('.', 2) : new[] { "public", tableName };
        var schema = parts[0];
        var table = parts[1];

        var result = new TableSchema { SchemaName = schema, TableName = table };

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        // Get columns
        await using (var cmd = new NpgsqlCommand(@"
            SELECT column_name, data_type, is_nullable, column_default, 
                   character_maximum_length, numeric_precision
            FROM information_schema.columns 
            WHERE table_name = @table AND table_schema = @schema
            ORDER BY ordinal_position", conn))
        {
            cmd.Parameters.AddWithValue("table", table);
            cmd.Parameters.AddWithValue("schema", schema);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Columns.Add(new ColumnSchema
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    HasDefaultValue = !reader.IsDBNull(3),
                    DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3),
                    MaxLength = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    NumericPrecision = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                });
            }
        }

        // Get primary keys
        await using (var cmd = new NpgsqlCommand(@"
            SELECT kcu.column_name 
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu 
              ON tc.constraint_name = kcu.constraint_name 
              AND tc.table_schema = kcu.table_schema
            WHERE tc.table_name = @table 
              AND tc.table_schema = @schema
              AND tc.constraint_type = 'PRIMARY KEY'", conn))
        {
            cmd.Parameters.AddWithValue("table", table);
            cmd.Parameters.AddWithValue("schema", schema);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var pkCol = reader.GetString(0);
                result.PrimaryKeys.Add(pkCol);
                var col = result.Columns.FirstOrDefault(c => c.Name == pkCol);
                if (col != null) col.IsPrimaryKey = true;
            }
        }

        // Get foreign keys -- collect raw data first, then resolve display columns
        var rawForeignKeys = new List<(string colName, string refTable, string refColumn)>();
        await using (var cmd = new NpgsqlCommand(@"
            SELECT kcu.column_name, ccu.table_schema || '.' || ccu.table_name, ccu.column_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu 
              ON tc.constraint_name = kcu.constraint_name
              AND tc.table_schema = kcu.table_schema
            JOIN information_schema.constraint_column_usage ccu 
              ON tc.constraint_name = ccu.constraint_name
              AND tc.table_schema = ccu.constraint_schema
            WHERE tc.table_name = @table 
              AND tc.table_schema = @schema
              AND tc.constraint_type = 'FOREIGN KEY'", conn))
        {
            cmd.Parameters.AddWithValue("table", table);
            cmd.Parameters.AddWithValue("schema", schema);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                rawForeignKeys.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }

        foreach (var (fkColName, refTable, refColumn) in rawForeignKeys)
        {
            var fk = new ForeignKeyInfo
            {
                ColumnName = fkColName,
                ReferencedTable = refTable,
                ReferencedColumn = refColumn,
                LookupDisplayColumn = await InferDisplayColumnAsync(conn, refTable)
            };

            result.ForeignKeys.Add(fk);

            var col = result.Columns.FirstOrDefault(c => c.Name == fkColName);
            if (col != null) col.IsForeignKey = true;
        }

        return result;
    }

    public async Task<object?> LookupValueAsync(string connectionString, string table, string matchColumn, string matchValue, string returnColumn)
    {
        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var sql = $"SELECT \"{returnColumn}\" FROM \"{parts[0]}\".\"{parts[1]}\" WHERE LOWER(\"{matchColumn}\") = LOWER(@val) LIMIT 1";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("val", matchValue);

        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object?> LookupByCompositeAsync(string connectionString, string table, IReadOnlyList<(string Column, object? Value)> matchPairs, string returnColumn)
    {
        if (matchPairs == null || matchPairs.Count == 0)
            return null;

        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        var qualifiedTable = $"\"{parts[0]}\".\"{parts[1]}\"";
        var conditions = new List<string>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        for (var i = 0; i < matchPairs.Count; i++)
        {
            var (col, val) = matchPairs[i];
            var paramName = $"p{i}";
            if (val == null || val == DBNull.Value)
            {
                conditions.Add($"\"{col}\" IS NULL");
            }
            else if (val is string s)
            {
                conditions.Add($"LOWER(\"{col}\") = LOWER(@{paramName})");
                cmd.Parameters.AddWithValue(paramName, s);
            }
            else
            {
                conditions.Add($"\"{col}\" = @{paramName}");
                cmd.Parameters.AddWithValue(paramName, val);
            }
        }
        cmd.CommandText = $"SELECT \"{returnColumn}\" FROM {qualifiedTable} WHERE {string.Join(" AND ", conditions)} LIMIT 1";
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object> CreateLookupRecordAsync(string connectionString, string table, string displayColumn, object displayValue, string pkColumn, object? pkValue = null)
    {
        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        string sql;
        if (pkValue != null)
        {
            sql = $"INSERT INTO \"{parts[0]}\".\"{parts[1]}\" (\"{pkColumn}\", \"{displayColumn}\") VALUES (@pk, @val) RETURNING \"{pkColumn}\"";
        }
        else
        {
            sql = $"INSERT INTO \"{parts[0]}\".\"{parts[1]}\" (\"{displayColumn}\") VALUES (@val) RETURNING \"{pkColumn}\"";
        }

        await using var cmd = new NpgsqlCommand(sql, conn);
        if (pkValue != null)
            cmd.Parameters.AddWithValue("pk", pkValue);
        cmd.Parameters.AddWithValue("val", displayValue);

        var result = await cmd.ExecuteScalarAsync();
        return result ?? throw new InvalidOperationException($"Failed to create lookup record in table \"{table}\". Ensure the primary key column is properly configured.");
    }

    public async Task<object> InsertSingleRecordAsync(string connectionString, string table, List<string> columns, List<object?> values, string returnColumn)
    {
        try
        {
            var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
            var qualifiedTable = $"\"{parts[0]}\".\"{parts[1]}\"";
            var quotedColumns = string.Join(", ", columns.Select(c => $"\"{c}\""));
            var paramNames = Enumerable.Range(0, columns.Count).Select(j => $"@p{j}").ToList();
            var sql = $"INSERT INTO {qualifiedTable} ({quotedColumns}) VALUES ({string.Join(", ", paramNames)}) RETURNING \"{returnColumn}\"";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            for (var j = 0; j < values.Count; j++)
                cmd.Parameters.AddWithValue($"p{j}", values[j] ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result ?? throw new InvalidOperationException($"Failed to insert record into table \"{table}\". Check that required columns are provided and values are valid.");
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            var matchPairs = new List<(string Column, object? Value)>();
            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].Equals(returnColumn, StringComparison.OrdinalIgnoreCase))
                    continue;
                matchPairs.Add((columns[i], values[i]));
            }
            if (matchPairs.Count == 0)
                throw;
            var existingId = await LookupByCompositeAsync(connectionString, table, matchPairs, returnColumn);
            if (existingId != null && existingId != DBNull.Value)
                return existingId;
            throw;
        }
    }

    public async Task<IDbTransactionScope> BeginTransactionAsync(string connectionString)
    {
        var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        var transaction = await conn.BeginTransactionAsync();
        return new PostgreSqlTransactionScope(conn, transaction);
    }

    public async Task<BatchInsertResult> BatchInsertAsync(string connectionString, string table, List<string> columns, List<List<object?>> rows, int batchSize = 1000)
    {
        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        var qualifiedTable = $"\"{parts[0]}\".\"{parts[1]}\"";
        var quotedColumns = string.Join(", ", columns.Select(c => $"\"{c}\""));
        var result = new BatchInsertResult();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        for (int i = 0; i < rows.Count; i += batchSize)
        {
            var batch = rows.Skip(i).Take(batchSize).ToList();
            
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                foreach (var row in batch)
                {
                    var paramNames = Enumerable.Range(0, columns.Count).Select(j => $"@p{j}").ToList();
                    var sql = $"INSERT INTO {qualifiedTable} ({quotedColumns}) VALUES ({string.Join(", ", paramNames)})";
                    
                    await using var cmd = new NpgsqlCommand(sql, conn, transaction);
                    for (int j = 0; j < row.Count; j++)
                    {
                        cmd.Parameters.AddWithValue($"p{j}", row[j] ?? DBNull.Value);
                    }
                    
                    await cmd.ExecuteNonQueryAsync();
                    result.InsertedCount++;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                var friendlyMessage = ex is PostgresException pgEx
                    ? TranslatePostgresError(pgEx)
                    : ex.Message;
                result.ErrorMessage = $"Batch starting at row {i + 1} failed: {friendlyMessage}";
                return result;
            }
        }

        result.Success = true;
        return result;
    }

    private static string TranslatePostgresError(PostgresException ex)
    {
        var table = ex.TableName ?? "unknown";
        var column = ex.ColumnName ?? "unknown";
        return ex.SqlState switch
        {
            "23502" => $"Column \"{column}\" in table \"{table}\" is required but was not provided.",
            "23503" => "A referenced record does not exist. Check that the value exists in the related table.",
            "23505" => $"A record with this value already exists. Duplicate key in table \"{table}\".",
            "23514" => "The value does not meet the check constraint.",
            _ => ex.Message
        };
    }

    private static async Task<string?> InferDisplayColumnAsync(NpgsqlConnection conn, string fullTableName)
    {
        var parts = fullTableName.Contains('.') ? fullTableName.Split('.', 2) : new[] { "public", fullTableName };
        var displayCandidates = new[] { "name", "title", "code", "description", "label", "display_name" };

        await using var cmd = new NpgsqlCommand(@"
            SELECT column_name FROM information_schema.columns 
            WHERE table_name = @table AND table_schema = @schema
            ORDER BY ordinal_position", conn);
        cmd.Parameters.AddWithValue("table", parts[1]);
        cmd.Parameters.AddWithValue("schema", parts[0]);

        var columnNames = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            columnNames.Add(reader.GetString(0));

        foreach (var candidate in displayCandidates)
        {
            var match = columnNames.FirstOrDefault(c => c.Equals(candidate, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }

        var textCol = columnNames.FirstOrDefault(c => 
            !c.Equals("id", StringComparison.OrdinalIgnoreCase) && 
            !c.EndsWith("_id", StringComparison.OrdinalIgnoreCase));
        
        return textCol;
    }
}

internal sealed class PostgreSqlTransactionScope : IDbTransactionScope
{
    private readonly NpgsqlConnection _conn;
    private readonly NpgsqlTransaction _transaction;
    private bool _committed;

    public PostgreSqlTransactionScope(NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        _conn = conn;
        _transaction = transaction;
    }

    public async Task<object?> LookupValueAsync(string table, string matchColumn, string matchValue, string returnColumn)
    {
        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        var sql = $"SELECT \"{returnColumn}\" FROM \"{parts[0]}\".\"{parts[1]}\" WHERE LOWER(\"{matchColumn}\") = LOWER(@val) LIMIT 1";
        await using var cmd = new NpgsqlCommand(sql, _conn, _transaction);
        cmd.Parameters.AddWithValue("val", matchValue);
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object?> LookupByCompositeAsync(string table, IReadOnlyList<(string Column, object? Value)> matchPairs, string returnColumn)
    {
        if (matchPairs == null || matchPairs.Count == 0)
            return null;

        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        var qualifiedTable = $"\"{parts[0]}\".\"{parts[1]}\"";
        var conditions = new List<string>();
        await using var cmd = new NpgsqlCommand();
        cmd.Connection = _conn;
        cmd.Transaction = _transaction;
        for (var i = 0; i < matchPairs.Count; i++)
        {
            var (col, val) = matchPairs[i];
            var paramName = $"p{i}";
            if (val == null || val == DBNull.Value)
            {
                conditions.Add($"\"{col}\" IS NULL");
            }
            else if (val is string s)
            {
                conditions.Add($"LOWER(\"{col}\") = LOWER(@{paramName})");
                cmd.Parameters.AddWithValue(paramName, s);
            }
            else
            {
                conditions.Add($"\"{col}\" = @{paramName}");
                cmd.Parameters.AddWithValue(paramName, val);
            }
        }
        cmd.CommandText = $"SELECT \"{returnColumn}\" FROM {qualifiedTable} WHERE {string.Join(" AND ", conditions)} LIMIT 1";
        return await cmd.ExecuteScalarAsync();
    }

    public async Task<object> CreateLookupRecordAsync(string table, string displayColumn, object displayValue, string pkColumn, object? pkValue)
    {
        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        string sql;
        if (pkValue != null)
        {
            sql = $"INSERT INTO \"{parts[0]}\".\"{parts[1]}\" (\"{pkColumn}\", \"{displayColumn}\") VALUES (@pk, @val) RETURNING \"{pkColumn}\"";
        }
        else
        {
            sql = $"INSERT INTO \"{parts[0]}\".\"{parts[1]}\" (\"{displayColumn}\") VALUES (@val) RETURNING \"{pkColumn}\"";
        }

        await using var cmd = new NpgsqlCommand(sql, _conn, _transaction);
        if (pkValue != null)
            cmd.Parameters.AddWithValue("pk", pkValue);
        cmd.Parameters.AddWithValue("val", displayValue);

        var result = await cmd.ExecuteScalarAsync();
        return result ?? throw new InvalidOperationException($"Failed to create lookup record in table \"{table}\". Ensure the primary key column is properly configured.");
    }

    public async Task<object> InsertSingleRecordAsync(string table, List<string> columns, List<object?> values, string returnColumn)
    {
        const string savepointName = "sp_insert_single";
        await _transaction.SaveAsync(savepointName);
        try
        {
            var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
            var qualifiedTable = $"\"{parts[0]}\".\"{parts[1]}\"";
            var quotedColumns = string.Join(", ", columns.Select(c => $"\"{c}\""));
            var paramNames = Enumerable.Range(0, columns.Count).Select(j => $"@p{j}").ToList();
            var sql = $"INSERT INTO {qualifiedTable} ({quotedColumns}) VALUES ({string.Join(", ", paramNames)}) RETURNING \"{returnColumn}\"";

            await using var cmd = new NpgsqlCommand(sql, _conn, _transaction);
            for (var j = 0; j < values.Count; j++)
                cmd.Parameters.AddWithValue($"p{j}", values[j] ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result ?? throw new InvalidOperationException($"Failed to insert record into table \"{table}\". Check that required columns are provided and values are valid.");
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            await _transaction.RollbackAsync(savepointName);
            var matchPairs = new List<(string Column, object? Value)>();
            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].Equals(returnColumn, StringComparison.OrdinalIgnoreCase))
                    continue;
                matchPairs.Add((columns[i], values[i]));
            }
            if (matchPairs.Count == 0)
                throw;
            var existingId = await LookupByCompositeAsync(table, matchPairs, returnColumn);
            if (existingId != null && existingId != DBNull.Value)
                return existingId;
            throw;
        }
    }

    public async Task InsertBatchAsync(string table, List<string> columns, List<List<object?>> rows)
    {
        var parts = table.Contains('.') ? table.Split('.', 2) : new[] { "public", table };
        var qualifiedTable = $"\"{parts[0]}\".\"{parts[1]}\"";
        var quotedColumns = string.Join(", ", columns.Select(c => $"\"{c}\""));

        foreach (var row in rows)
        {
            var paramNames = Enumerable.Range(0, columns.Count).Select(j => $"@p{j}").ToList();
            var sql = $"INSERT INTO {qualifiedTable} ({quotedColumns}) VALUES ({string.Join(", ", paramNames)})";

            await using var cmd = new NpgsqlCommand(sql, _conn, _transaction);
            for (int j = 0; j < row.Count; j++)
            {
                cmd.Parameters.AddWithValue($"p{j}", row[j] ?? DBNull.Value);
            }

            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task CommitAsync()
    {
        await _transaction.CommitAsync();
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            await _transaction.RollbackAsync();
        }

        await _transaction.DisposeAsync();
        await _conn.DisposeAsync();
    }
}
