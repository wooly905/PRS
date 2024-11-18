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
                            SELECT TABLE_SCHEMA,
                                TABLE_NAME,
                                COLUMN_NAME,
                                ORDINAL_POSITION,
                                COLUMN_DEFAULT,
                                IS_NULLABLE,
                                DATA_TYPE,
                                CHARACTER_MAXIMUM_LENGTH
                            FROM INFORMATION_SCHEMA.COLUMNS
                            ORDER BY TABLE_SCHEMA,
                                TABLE_NAME,
                                COLUMN_NAME
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
                CharacterMaximumLength = reader[7].ToString()
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
