namespace PRS.Tests.TestHelpers;

/// <summary>
/// Helper class for managing test files.
/// </summary>
public static class TestFileHelper
{
    private static readonly string TestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
    private static string _baseTempPath = null;

    public static string GetTestDataPath(string fileName)
    {
        return Path.Combine(TestDataPath, fileName);
    }

    public static string GetTempPath()
    {
        if (_baseTempPath == null)
        {
            _baseTempPath = Path.Combine(Path.GetTempPath(), "PRS.Tests", Guid.NewGuid().ToString());
        }

        if (!Directory.Exists(_baseTempPath))
        {
            Directory.CreateDirectory(_baseTempPath);
            // Create common subdirectories
            Directory.CreateDirectory(Path.Combine(_baseTempPath, ".prs"));
            Directory.CreateDirectory(Path.Combine(_baseTempPath, ".prs", "schemas"));
            Directory.CreateDirectory(Path.Combine(_baseTempPath, "schemas"));
        }
        return _baseTempPath;
    }

    public static string GetTempFilePath(string fileName)
    {
        var filePath = Path.Combine(GetTempPath(), fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return filePath;
    }

    public static void CleanupTempFiles()
    {
        try
        {
            if (_baseTempPath != null && Directory.Exists(_baseTempPath))
            {
                // Give some time for file handles to be released
                System.Threading.Thread.Sleep(100);
                Directory.Delete(_baseTempPath, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public static void CopyTestFile(string sourceFileName, string destFileName)
    {
        var sourcePath = GetTestDataPath(sourceFileName);
        var destPath = GetTempFilePath(destFileName);
        
        // Ensure destination directory exists
        var destDir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        
        File.Copy(sourcePath, destPath, true);
    }

    public static string CreateTestSchemaFile(string fileName)
    {
        var testSchemaPath = GetTestDataPath(fileName);
        var tempPath = GetTempFilePath(fileName);
        File.Copy(testSchemaPath, tempPath, true);
        return tempPath;
    }

    public static void ResetTempPath()
    {
        CleanupTempFiles();
        _baseTempPath = null;
    }
}

