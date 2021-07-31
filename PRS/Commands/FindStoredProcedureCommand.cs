﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands
{
    internal class FindStoredProcedureCommand : ICommand
    {
        private readonly IDisplay _display;
        private readonly IFileProvider _fileProvider;

        public FindStoredProcedureCommand(IDisplay display, IFileProvider fileProvider)
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
                _display.ShowInfo("prs fsp [stored procedure name]");
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

                if (string.Equals(line, Global.StoredProcedureSectionName))
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
            List<string> models = new();

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

                if (line?.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    models.Add(line);
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

        private void PrintModel(List<string> models)
        {
            _display.ShowInfo("Stored Procedure Name");

            foreach (string m in models)
            {
                _display.ShowInfo(m);
            }
        }
    }
}
