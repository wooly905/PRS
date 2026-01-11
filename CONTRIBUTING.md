# Contributing to PRS

We love your input! We want to make contributing to this project as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## We Develop with Github

We use GitHub to host code, track issues, request features, and accept pull requests.

## Development Setup

### Prerequisites
- .NET 10.0 SDK or higher
- SQL Server (for database connectivity)
- Git

### Getting Started

1. Fork and clone the repository
2. Build the project:
   ```bash
   dotnet build src/PRS.csproj
   ```
3. Run tests:
   ```bash
   dotnet test tests/PRS.Tests.csproj
   ```

### Running Tests

We have comprehensive test coverage (100% pass rate):

```bash
# Run all tests
cd tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~FindTableCommandTests"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

See [tests/README.md](tests/README.md) for more details.

## Code Changes

### Before Submitting a PR

1. **Run all tests** - Ensure 100% pass rate:
   ```bash
   dotnet test tests/PRS.Tests.csproj
   ```

2. **Check for linter errors**:
   ```bash
   dotnet build src/PRS.csproj
   ```

3. **Add tests for new features** - Maintain 80%+ coverage

4. **Update documentation** if needed

### Adding New Commands

When adding a new command:

1. Create the command class in `src/Commands/`
2. Implement `ICommand` interface
3. Add to `CommandProvider.cs`
4. **Write tests** in `tests/Commands/`
5. Aim for at least 8-10 test cases covering:
   - Normal operation
   - Error cases (null, missing args)
   - Edge cases
   - Partial string search (if applicable)

Example test structure:
```csharp
public class MyCommandTests : IDisposable
{
    [Fact]
    public async Task RunAsync_WithValidInput_Succeeds() { ... }
    
    [Fact]
    public async Task RunAsync_WithNullArguments_ShowsError() { ... }
    
    public void Dispose() 
    { 
        TestFileHelper.CleanupTempFiles(); 
    }
}
```

## Great Bug Reports

A great bug report includes:

- **Quick summary** - One-line description
- **Steps to reproduce**
  - Be specific!
  - Provide sample commands
  - Include your environment (OS, .NET version)
- **Expected behavior**
- **Actual behavior**
- **Additional notes** - Why you think it's happening, what you tried

## Use a Consistent Coding Style

Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### Key Points
- Use meaningful variable names
- Follow existing patterns in the codebase
- Add XML documentation comments for public APIs
- Keep methods focused and small
- Use `async`/`await` for I/O operations

## Pull Request Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. **Run tests** (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### PR Requirements
- ✅ All tests must pass (100%)
- ✅ No new linter errors
- ✅ Add tests for new features
- ✅ Update documentation if needed
- ✅ Follow existing code style

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
