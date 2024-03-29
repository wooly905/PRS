﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal class FindTableCommand : ICommand
{
    private readonly IDisplay _display;
    private readonly IFileProvider _fileProvider;

    public FindTableCommand(IDisplay display, IFileProvider fileProvider)
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
            _display.ShowInfo("prs ft [table name]");
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
            string line = await reader.ReadLineAsync();

            if (line == null)
            {
                // end of file
                break;
            }

            if (string.Equals(line, Global.TableSectionName))
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
        List<TableModel> models = new();

        while (true)
        {
            string line = await reader.ReadLineAsync();

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

            if (splits[1]?.IndexOf(args[1], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                TableModel m = new()
                {
                    TableSchema = splits[0],
                    TableName = splits[1],
                    TableType = splits[2]
                };
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
