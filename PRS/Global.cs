using System;
using System.IO;

namespace PRS;

internal static class Global
{
    public static string SchemaFileName => "schema.txt";
    public static string ConnectionStringFileName => "prs.txt";
    public static string SchemaFileDirectory => Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".prs");
    public static string SchemaFilePath => Path.Combine(SchemaFileDirectory, SchemaFileName);
    public static string ConnectionStringFilePath => Path.Combine(SchemaFileDirectory, ConnectionStringFileName);
    public static string ConnectionStringSectionName => "[CS]";
    public static string TableSectionName => "[Table]";
    public static string ColumnSectionName => "[Column]";
    public static string StoredProcedureSectionName => "[StoredProcedure]";
}
