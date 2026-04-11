using PRS.Database;

namespace PRS.Display;

internal interface IDisplay
{
    void ShowInfo(string message);

    void ShowError(string message);

    void DisplayColumns(IEnumerable<ColumnModel> models, OutputFormat format = OutputFormat.Table);

    void DisplayTableSchema(IEnumerable<ColumnModel> columns, string tableName, OutputFormat format = OutputFormat.Table);

    void DisplayTables(IEnumerable<TableModel> models, OutputFormat format = OutputFormat.Table);

    void DisplayStoredProcedures(IEnumerable<string> procedures, OutputFormat format = OutputFormat.Table);
}
