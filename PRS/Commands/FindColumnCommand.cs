﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands
{
    internal class FindColumnCommand : ICommand
    {
        private readonly IDisplay _display;
        private readonly IFileProvider _fileProvider;

        public FindColumnCommand(IDisplay display, IFileProvider fileProvider)
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
                _display.ShowInfo("prs fc [column name]");
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

                if (splits[2]?.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0)
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
                PrintModel(models);
            }
            else
            {
                _display.ShowInfo("Nothing found.");
            }
        }

        private void PrintModel(List<ColumnModel> models)
        {
            _display.ShowInfo("TABLE_SCHEMA   TABLE_NAME                      COLUMN_NAME                            ORDINAL_POSITION    COLUMN_DEFAULT     IS_NULLABLE     DATA_TYPE       MAX_LENGTH");

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
                _display.ShowInfo(output);
            }
        }
    }
}
