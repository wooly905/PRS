# PRS Tests

This directory contains the automated test suite for the PRS project. We use [xUnit](https://xunit.net/) as our testing framework and [Moq](https://github.com/moq/moq4) for mocking dependencies.

## Structure

- `Commands/`: Tests for the CLI commands.
- `FileHandle/`: Tests for schema reading and writing logic.
- `Mcp/`: Tests for the MCP server components.
- `Services/`: Tests for business logic services.
- `TestHelpers/`: Mock implementations and helper classes for testing.
- `TestData/`: Sample schema files used in tests.

## Running Tests

### Run All Tests

To run all tests in the solution:

```bash
dotnet test
```

### Run Specific Test Project

To run only the core tests:

```bash
dotnet test tests/PRS.Tests.csproj
```

To run only the MCP server tests:

```bash
dotnet test tests/src-mcp/PRS.McpServer.Tests.csproj
```

### Run Specific Tests

You can filter tests by name:

```bash
dotnet test --filter "FullyQualifiedName~FindTableCommandTests"
```

## Adding New Tests

When adding new features, please ensure you include corresponding tests. We aim for:
- 100% pass rate.
- Coverage for normal, error, and edge cases.
- Use of `TestFileHelper` for managing temporary files during tests.

### Example Test Class

```csharp
public class MyFeatureTests : IDisposable
{
    [Fact]
    public async Task Feature_WorksAsExpected()
    {
        // Arrange
        // Act
        // Assert
    }

    public void Dispose()
    {
        TestFileHelper.CleanupTempFiles();
    }
}
```
