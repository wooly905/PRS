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
                            SELECT c.TABLE_SCHEMA,
                                   c.TABLE_NAME,
                                   c.COLUMN_NAME,
                                   c.ORDINAL_POSITION,
                                   c.COLUMN_DEFAULT,
                                   c.IS_NULLABLE,
                                   c.DATA_TYPE,
                                   c.CHARACTER_MAXIMUM_LENGTH,
                                   fk.name AS ForeignKeyName,
                                   OBJECT_SCHEMA_NAME(fkc.referenced_object_id) AS ReferencedTableSchema,
                                   OBJECT_NAME(fkc.referenced_object_id) AS ReferencedTableName,
                                   rc.name AS ReferencedColumnName
                            FROM INFORMATION_SCHEMA.COLUMNS c
                            LEFT JOIN sys.columns sc
                                ON sc.object_id = OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME))
                               AND sc.name = c.COLUMN_NAME
                            LEFT JOIN sys.foreign_key_columns fkc
                                ON fkc.parent_object_id = sc.object_id
                               AND fkc.parent_column_id = sc.column_id
                            LEFT JOIN sys.foreign_keys fk
                                ON fk.object_id = fkc.constraint_object_id
                            LEFT JOIN sys.columns rc
                                ON rc.object_id = fkc.referenced_object_id
                               AND rc.column_id = fkc.referenced_column_id
                            ORDER BY c.TABLE_SCHEMA,
                                     c.TABLE_NAME,
                                     c.COLUMN_NAME
                            """;
        command.CommandType = CommandType.Text;
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        List<ColumnModel> columns = [];

        while (await reader.ReadAsync())
        {
            ColumnModel m = new()
            {
                TableSchema = reader[0].ToString(),
                TableName = reader[1].ToString(),
                ColumnName = reader[2].ToString(),
                OrdinalPosition = reader[3].ToString(),
                ColumnDefault = reader[4].ToString(),
                IsNullable = reader[5].ToString(),
                DataType = reader[6].ToString(),
                CharacterMaximumLength = reader[7].ToString(),
                ForeignKeyName = reader.IsDBNull(8) ? null : reader[8].ToString(),
                ReferencedTableSchema = reader.IsDBNull(9) ? null : reader[9].ToString(),
                ReferencedTableName = reader.IsDBNull(10) ? null : reader[10].ToString(),
                ReferencedColumnName = reader.IsDBNull(11) ? null : reader[11].ToString()
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
        command.CommandText = "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME";
        command.CommandType = CommandType.Text;
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        List<TableModel> tables = [];

        while (await reader.ReadAsync())
        {
            TableModel m = new()
            {
                TableSchema = reader[0].ToString(),
                TableName = reader[1].ToString(),
                TableType = reader[2].ToString()
            };
            tables.Add(m);
        }

        reader.Close();
        connection.Close();

        return tables;
    }
}
