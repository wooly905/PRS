using PRS.Database;

namespace PRS.Tests.TestHelpers;

/// <summary>
/// Mock implementation of IDatabase for testing.
/// </summary>
public class MockDatabase : IDatabase
{
    public List<TableModel> Tables { get; set; } = new();
    public List<ColumnModel> Columns { get; set; } = new();
    public List<string> StoredProcedures { get; set; } = new();

    public Task<IEnumerable<ColumnModel>> GetColumnModelsAsync(string connectionString)
    {
        return Task.FromResult<IEnumerable<ColumnModel>>(Columns);
    }

    public Task<IEnumerable<string>> GetStoredProcedureNamesAsync(string connectionString)
    {
        return Task.FromResult<IEnumerable<string>>(StoredProcedures);
    }

    public Task<IEnumerable<TableModel>> GetTableModelsAsync(string connectionString)
    {
        return Task.FromResult<IEnumerable<TableModel>>(Tables);
    }

    public static MockDatabase CreateTestData()
    {
        var mock = new MockDatabase();

        // Add test tables
        mock.Tables.Add(new TableModel { TableName = "Users", TableType = "BASE TABLE" });
        mock.Tables.Add(new TableModel { TableName = "Orders", TableType = "BASE TABLE" });
        mock.Tables.Add(new TableModel { TableName = "Products", TableType = "BASE TABLE" });

        // Add test columns
        mock.Columns.Add(new ColumnModel
        {
            TableName = "Users",
            ColumnName = "UserId",
            OrdinalPosition = "1",
            IsNullable = "NO",
            DataType = "int",
            ColumnDefault = "",
            CharacterMaximumLength = "",
            ForeignKeyName = "",
            ReferencedTableName = "",
            ReferencedColumnName = ""
        });

        mock.Columns.Add(new ColumnModel
        {
            TableName = "Users",
            ColumnName = "UserName",
            OrdinalPosition = "2",
            IsNullable = "NO",
            DataType = "nvarchar",
            ColumnDefault = "",
            CharacterMaximumLength = "100",
            ForeignKeyName = "",
            ReferencedTableName = "",
            ReferencedColumnName = ""
        });

        mock.Columns.Add(new ColumnModel
        {
            TableName = "Orders",
            ColumnName = "OrderId",
            OrdinalPosition = "1",
            IsNullable = "NO",
            DataType = "int",
            ColumnDefault = "",
            CharacterMaximumLength = "",
            ForeignKeyName = "",
            ReferencedTableName = "",
            ReferencedColumnName = ""
        });

        mock.Columns.Add(new ColumnModel
        {
            TableName = "Orders",
            ColumnName = "UserId",
            OrdinalPosition = "2",
            IsNullable = "NO",
            DataType = "int",
            ColumnDefault = "",
            CharacterMaximumLength = "",
            ForeignKeyName = "FK_Orders_Users",
            ReferencedTableName = "Users",
            ReferencedColumnName = "UserId"
        });

        // Add test stored procedures
        mock.StoredProcedures.Add("sp_GetUsers");
        mock.StoredProcedures.Add("sp_GetOrders");
        mock.StoredProcedures.Add("sp_CreateOrder");

        return mock;
    }
}

