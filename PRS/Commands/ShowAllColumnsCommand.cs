using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands
{
    internal class ShowAllColumnsCommand : ICommand
    {
        private readonly IDisplay _display;
        private readonly IFileProvider _fileProvider;

        public ShowAllColumnsCommand(IDisplay display, IFileProvider fileProvider)
        {
            _display = display;
            _fileProvider = fileProvider;
        }

        public async Task RunAsync(string[] args)
        {
            // verify args
            if (args == null || args.Length != 2)
            {
                _display.ShowError("Argument mismatch");
                _display.ShowInfo("prs sc [table name]");
                return;
            }

            // verify schema file exists. if not, show no schema file error and ask to run dump command.
            if (!File.Exists(Global.SchemaFilePath))
            {
                _display.ShowError("Schema doesn't exist locally. Please run dds command first.");
                return;
            }

            // read schema file line by line and search table and column 
            IFileReader reader = _fileProvider.GetFileReader(Global.SchemaFilePath);
            bool found = false;

            while (true)
            {
                string line = await reader.ReadLineAsync().ConfigureAwait(false);

                if (line == null)
                {
                    // end of file
                    break;
                }

                if (string.Equals(line, Global.ColumnSectionName))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _display.ShowInfo("Nothing found");
                return;
            }

            // find targets 
            List<ColumnModel> models = new();

            while (true)
            {
                string line = await reader.ReadLineAsync().ConfigureAwait(false);

                if (line == null)
                {
                    // end of file
                    break;
                }

                if (line.StartsWith("["))
                {
                    // reach other section
                    break;
                }

                string[] splits = line.Split(new char[] { ',' });

                if (string.Equals(splits[1], args[1], StringComparison.OrdinalIgnoreCase))
                {
                    ColumnModel m = new();
                    m.TableSchema = splits[0];
                    m.TableName = splits[1];
                    m.ColumnName = splits[2];
                    m.OrdinalPosition = splits[3];
                    m.ColumnDefault = splits[4];
                    m.IsNullable = splits[5];
                    m.DataType = splits[6];
                    m.CharacterMaximumLength = splits[7];
                    models.Add(m);
                }
            }

            reader.Dispose();

            if (models.Count > 0)
            {
                CommandHelper.PrintModel(models, _display);
            }
            else
            {
                _display.ShowInfo("Nothing found.");
            }
        }

    }
}
