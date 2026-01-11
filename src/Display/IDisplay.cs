using PRS.Database;

namespace PRS.Display;

internal interface IDisplay
{
    void ShowInfo(string message);

    void ShowError(string message);

    void DisplayColumns(IEnumerable<ColumnModel> models);

    void DisplayTables(IEnumerable<TableModel> models);
}
