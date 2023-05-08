using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal static class CommandHelper
{
    public static async Task<string> GetConnectionStringAsync(IDisplay display, IFileProvider fileProvider)
    {
        if (!File.Exists(Global.ConnectionStringFilePath))
        {
            display.ShowError("Connection string doesn't exist. Please set it first.");
            return string.Empty;
        }

        IFileReader reader = fileProvider.GetFileReader(Global.ConnectionStringFilePath);
        string output = await reader.ReadLineAsync();
        reader.Dispose();

        return output;
    }

    public static void PrintModel(IEnumerable<ColumnModel> models, IDisplay display)
    {
        display.ShowInfo("TABLE_SCHEMA   TABLE_NAME                      COLUMN_NAME                            ORDINAL_POSITION    COLUMN_DEFAULT     IS_NULLABLE     DATA_TYPE       MAX_LENGTH");

        foreach (ColumnModel m in models)
        {
            string output = string.Format("{0,-15}{1,-32}{2,-39}{3,-20}{4,-19}{5,-16}{6,-16}{7,-10}",
                                          m.TableSchema,
                                          m.TableName,
                                          m.ColumnName,
                                          m.OrdinalPosition,
                                          m.ColumnDefault,
                                          m.IsNullable,
                                          m.DataType,
                                          m.CharacterMaximumLength);
            display.ShowInfo(output);
        }
    }

    public static void PrintModel(IEnumerable<TableModel> models, IDisplay display)
    {
        display.ShowInfo("TABLE_SCHEMA   TABLE_NAME                                TABLE_TYPE");

        foreach (TableModel m in models)
        {
            string output = string.Format("{0,-15}{1,-42}{2,-10}",
                                          m.TableSchema,
                                          m.TableName,
                                          m.TableType);
            display.ShowInfo(output);
        }
    }

    public static void PrintModel(IEnumerable<string> models, IDisplay display)
    {
        display.ShowInfo("Name ===");

        foreach (string m in models)
        {
            display.ShowInfo(m);
        }
    }
}
