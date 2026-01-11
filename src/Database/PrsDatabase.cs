using System.Data;
using Microsoft.Data.SqlClient;

namespace PRS.Database;

internal class PrsDatabase : IDatabase
{
    public PrsDatabase()
    {
    }

    public async Task<IEnumerable<ColumnModel>> GetColumnModelsAsync(string connectionString)
    {
        using SqlConnection connection = new();
        connection.ConnectionString = connectionString;
        await connection.OpenAsync();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
                            SELECT c.TABLE_NAME,
                                   c.COLUMN_NAME,
                                   c.ORDINAL_POSITION,
                                   c.COLUMN_DEFAULT,
                                   c.IS_NULLABLE,
                                   c.DATA_TYPE,
                                   c.CHARACTER_MAXIMUM_LENGTH,
                                   fk.name AS ForeignKeyName,
                                   OBJECT_NAME(fkc.referenced_object_id) AS ReferencedTableName,
                                   rc.name AS ReferencedColumnName,
                                   sc.is_identity,
                                   ic.seed_value,
                                   ic.increment_value,
                                   CAST(CASE WHEN EXISTS (
                                       SELECT 1 FROM sys.index_columns ic2
                                       JOIN sys.indexes i ON ic2.object_id = i.object_id AND ic2.index_id = i.index_id
                                       WHERE ic2.object_id = sc.object_id AND ic2.column_id = sc.column_id AND i.is_primary_key = 1
                                   ) THEN 1 ELSE 0 END AS BIT) AS IsPrimaryKey,
                                   CAST(CASE WHEN EXISTS (
                                       SELECT 1 FROM sys.index_columns ic2
                                       JOIN sys.indexes i ON ic2.object_id = i.object_id AND ic2.index_id = i.index_id
                                       WHERE ic2.object_id = sc.object_id AND ic2.column_id = sc.column_id AND i.is_unique = 1
                                   ) THEN 1 ELSE 0 END AS BIT) AS IsUnique
                            FROM INFORMATION_SCHEMA.COLUMNS c
                            LEFT JOIN sys.columns sc
                                ON sc.object_id = OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME))
                               AND sc.name = c.COLUMN_NAME
                            LEFT JOIN sys.identity_columns ic
                                ON ic.object_id = sc.object_id
                               AND ic.column_id = sc.column_id
                            LEFT JOIN sys.foreign_key_columns fkc
                                ON fkc.parent_object_id = sc.object_id
                               AND fkc.parent_column_id = sc.column_id
                            LEFT JOIN sys.foreign_keys fk
                                ON fk.object_id = fkc.constraint_object_id
                            LEFT JOIN sys.columns rc
                                ON rc.object_id = fkc.referenced_object_id
                               AND rc.column_id = fkc.referenced_column_id
                            ORDER BY c.TABLE_NAME,
                                     c.COLUMN_NAME
                            """;
        command.CommandType = CommandType.Text;
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        List<ColumnModel> columns = [];

        while (await reader.ReadAsync())
        {
            ColumnModel m = new()
            {
                TableName = reader[0].ToString(),
                ColumnName = reader[1].ToString(),
                OrdinalPosition = reader[2].ToString(),
                ColumnDefault = reader[3].ToString(),
                IsNullable = reader[4].ToString(),
                DataType = reader[5].ToString(),
                CharacterMaximumLength = reader[6].ToString(),
                ForeignKeyName = reader.IsDBNull(7) ? null : reader[7].ToString(),
                ReferencedTableName = reader.IsDBNull(8) ? null : reader[8].ToString(),
                ReferencedColumnName = reader.IsDBNull(9) ? null : reader[9].ToString(),
                IsIdentity = !reader.IsDBNull(10) && (bool)reader[10],
                IdentitySeed = reader.IsDBNull(11) ? null : reader[11].ToString(),
                IdentityIncrement = reader.IsDBNull(12) ? null : reader[12].ToString(),
                IsPrimaryKey = !reader.IsDBNull(13) && (bool)reader[13],
                IsUnique = !reader.IsDBNull(14) && (bool)reader[14]
            };
            columns.Add(m);
        }

        reader.Close();
        connection.Close();

        return columns;
    }

    public async Task<IEnumerable<string>> GetStoredProcedureNamesAsync(string connectionString)
    {
        using SqlConnection connection = new();
        connection.ConnectionString = connectionString;
        await connection.OpenAsync();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Name FROM SYSOBJECTS WHERE type = 'P' AND category = 0 ORDER BY name";
        command.CommandType = CommandType.Text;
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        List<string> names = [];

        while (await reader.ReadAsync())
        {
            names.Add(reader[0].ToString());
        }

        reader.Close();
        connection.Close();

        return names;
    }

    public async Task<IEnumerable<TableModel>> GetTableModelsAsync(string connectionString)
    {
        using SqlConnection connection = new();
        connection.ConnectionString = connectionString;
        await connection.OpenAsync();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME";
        command.CommandType = CommandType.Text;
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        List<TableModel> tables = [];

        while (await reader.ReadAsync())
        {
            TableModel m = new()
            {
                TableName = reader[0].ToString(),
                TableType = reader[1].ToString()
            };
            tables.Add(m);
        }

        reader.Close();
        connection.Close();

        return tables;
    }
}
