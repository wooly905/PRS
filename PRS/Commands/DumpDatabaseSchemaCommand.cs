using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands
{
    internal class DumpDatabaseSchemaCommand : ICommand
    {
        private readonly IDisplay _display;
        private readonly IDatabase _database;
        private readonly IFileProvider _fileProvider;

        public DumpDatabaseSchemaCommand(IDisplay display, IDatabase database, IFileProvider fileProvider)
        {
            _display = display;
            _database = database;
            _fileProvider = fileProvider;
        }

        public async Task RunAsync(string[] args)
        {
            // verify default directory. create the directory.
            if (Directory.Exists(Global.SchemaFileDirectory))
            {
                if (File.Exists(Path.Combine(Global.SchemaFileDirectory, Global.SchemaFileName)))
                {
                    File.Delete(Path.Combine(Global.SchemaFileDirectory, Global.SchemaFileName));
                }
            }
            else
            {
                Directory.CreateDirectory(Global.SchemaFileDirectory);
            }

            // run database tool to get data
            string connectionString = await CommandHelper.GetConnectionStringAsync(_display, _fileProvider).ConfigureAwait(false);

            if (connectionString == null)
            {
                _display.ShowError("Connection string is not found.");
                return;
            }

            IEnumerable<TableModel> tables = await _database.GetTableModelsAsync(connectionString).ConfigureAwait(false);
            IEnumerable<ColumnModel> columns = await _database.GetColumnModelsAsync(connectionString).ConfigureAwait(false);
            IEnumerable<string> sps = await _database.GetStoredProcedureNamesAsync(connectionString).ConfigureAwait(false);

            // insert connection string and all data to file
            IFileWriter writer = _fileProvider.GetFileWriter(Path.Combine(Global.SchemaFileDirectory, Global.SchemaFileName));

            await WriteConnectionStringAsync(writer, connectionString).ConfigureAwait(false);
            await WriteTablesAsync(writer, tables).ConfigureAwait(false);
            await WriteColumnsAsync(writer, columns).ConfigureAwait(false);
            await WriteStoredProceduresAsync(writer, sps).ConfigureAwait(false);

            writer.Dispose();

            _display.ShowInfo("Dump database schema has been done.");
        }

        private async Task WriteConnectionStringAsync(IFileWriter writer, string connectionString)
        {
            await writer.WriteLineAsync(Global.ConnectionStringSectionName).ConfigureAwait(false);
            await writer.WriteLineAsync(connectionString).ConfigureAwait(false);
        }

        private async Task WriteTablesAsync(IFileWriter writer, IEnumerable<TableModel> tables)
        {
            await writer.WriteLineAsync(Global.TableSectionName).ConfigureAwait(false);

            foreach (TableModel m in tables)
            {
                string s = $"{m.TableSchema},{m.TableName},{m.TableType}";
                await writer.WriteLineAsync(s).ConfigureAwait(false);
            }
        }

        private async Task WriteColumnsAsync(IFileWriter writer, IEnumerable<ColumnModel> columns)
        {
            await writer.WriteLineAsync(Global.ColumnSectionName).ConfigureAwait(false);

            foreach (ColumnModel m in columns)
            {
                string s = $"{m.TableSchema},{m.TableName},{m.ColumnName},{m.OrdinalPosition},{m.ColumnDefault},{m.IsNullable},{m.DataType},{m.CharacterMaximumLength}";
                await writer.WriteLineAsync(s).ConfigureAwait(false);
            }
        }

        private async Task WriteStoredProceduresAsync(IFileWriter writer, IEnumerable<string> sps)
        {
            await writer.WriteLineAsync(Global.StoredProcedureSectionName).ConfigureAwait(false);

            foreach (string m in sps)
            {
                await writer.WriteLineAsync(m).ConfigureAwait(false);
            }
        }
    }
}
