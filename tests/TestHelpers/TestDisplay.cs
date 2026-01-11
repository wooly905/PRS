using PRS.Database;
using PRS.Display;

namespace PRS.Tests.TestHelpers;

/// <summary>
/// Test implementation of IDisplay that captures messages for assertion.
/// </summary>
public class TestDisplay : IDisplay
{
    public List<string> InfoMessages { get; } = new();
    public List<string> ErrorMessages { get; } = new();
    public List<string> AllMessages { get; } = new();

    public void ShowError(string message)
    {
        ErrorMessages.Add(message);
        AllMessages.Add($"[ERROR] {message}");
    }

    public void ShowInfo(string message)
    {
        InfoMessages.Add(message);
        AllMessages.Add($"[INFO] {message}");
    }

    public void DisplayColumns(IEnumerable<ColumnModel> models)
    {
        foreach (var model in models)
        {
            var message = $"Column: {model.TableName}.{model.ColumnName} ({model.DataType})";
            InfoMessages.Add(message);
            AllMessages.Add($"[INFO] {message}");
        }
    }

    public void DisplayTables(IEnumerable<TableModel> models)
    {
        foreach (var model in models)
        {
            var message = $"Table: {model.TableName} ({model.TableType})";
            InfoMessages.Add(message);
            AllMessages.Add($"[INFO] {message}");
        }
    }

    public void Clear()
    {
        InfoMessages.Clear();
        ErrorMessages.Clear();
        AllMessages.Clear();
    }

    public bool ContainsInfo(string text)
    {
        return InfoMessages.Any(m => m.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    public bool ContainsError(string text)
    {
        return ErrorMessages.Any(m => m.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    public bool ContainsAnyMessage(string text)
    {
        return AllMessages.Any(m => m.Contains(text, StringComparison.OrdinalIgnoreCase));
    }
}

