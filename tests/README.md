# PRS Tests

This is the unit test project for PRS (PostgreSQL/SQL Server Schema Explorer).

## 🎯 Test Goals

- **Test Coverage Target:** 80%+ 
- **Test Framework:** xUnit
- **Mocking Framework:** Moq

## 📁 Project Structure

```
tests/
├── PRS.Tests.csproj           # Test project file
├── xunit.runner.json          # xUnit configuration
├── TestData/                  # Test data directory
│   └── test.schema.xml        # Test XML schema
├── TestHelpers/               # Test helper classes
│   ├── TestDisplay.cs         # IDisplay test implementation
│   ├── TestFileHelper.cs      # File operation helpers
│   └── MockDatabase.cs        # IDatabase mock implementation
├── Commands/                  # Command tests
│   ├── FindTableCommandTests.cs
│   ├── FindColumnCommandTests.cs
│   ├── ShowAllColumnsCommandTests.cs
│   ├── FindTableColumnCommandTests.cs
│   ├── FindStoredProcedureCommandTests.cs
│   ├── DumpDatabaseSchemaCommandTests.cs
│   ├── UseSchemaCommandTests.cs
│   ├── ListSchemasCommandTests.cs
│   ├── RemoveSchemaCommandTests.cs
│   ├── ConnectionStringCommandTests.cs
│   ├── LlmCommandTests.cs
│   └── ErdCommandTests.cs
├── FileHandle/                # FileHandle tests
│   ├── XmlSchemaReaderTests.cs
│   └── XmlSchemaWriterTests.cs
└── Services/                  # Services tests
    └── SchemaSearchServiceTests.cs
```

## 🚀 Running Tests

### Run all tests
```bash
cd tests
dotnet test
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~FindTableCommandTests"
```

### Run tests with detailed output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Generate code coverage report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Test Coverage

### ✅ Commands (Tested)
- **FindTableCommand** - 8 test cases
  - Partial string search
  - Case insensitive
  - Not found results
  - Parameter validation
  
- **FindColumnCommand** - 8 test cases
  - Partial string search
  - Search across multiple tables
  - Case insensitive
  
- **ShowAllColumnsCommand** - 9 test cases
  - Exact match (no partial strings)
  - Foreign Key information
  - View columns
  
- **FindTableColumnCommand** - 8 test cases
  - Search both table and column (partial strings)
  - Combined filtering
  
- **FindStoredProcedureCommand** - 8 test cases
  - Partial string search
  - Different prefixes (sp_, usp_)
  
- **DumpDatabaseSchemaCommand** - 9 test cases
  - Create XML schema file
  - Write tables, columns, stored procedures
  - Foreign Key information
  - File overwrite
  
- **UseSchemaCommand** - 6 test cases
  - Switch active schema
  - Create active pointer file
  
- **ListSchemasCommand** - 4 test cases
  - List all schemas
  - Mark active schema
  
- **RemoveSchemaCommand** - 5 test cases
  - Delete schema file
  
- **Connection String Commands** - 6 test cases
  - Read and write connection string
  
- **LLM Commands** - 10 test cases
  - Set and read LLM URL and API Key
  
- **ErdCommand** - 3 test cases
  - Parameter validation (UI portion difficult to test)

### ✅ FileHandle (Tested)
- **XmlSchemaReader** - 15 test cases
  - Read connection string, tables, columns, procedures
  - Partial string search (FindTablesAsync, FindColumnsAsync)
  - Exact match (ReadColumnsForTableAsync)
  - Foreign Key information
  
- **XmlSchemaWriter** - 9 test cases
  - Write XML document
  - Nested structure (Tables -> Columns)
  - Foreign Key information
  - Null handling

### ✅ Services (Tested)
- **SchemaSearchService** - 12 test cases
  - Build schema context
  - JSON filtering (table/column hints)
  - SQL validation
  - Partial string matching

### ❌ Not Tested
- **AiCommand** - Not tested per requirements
- **Terminal.Gui UI** - Requires interactive environment, difficult for unit testing

## 📝 Test Statistics

| Category | Test Classes | Test Cases | Coverage Target |
|----------|-------------|------------|-----------------|
| Commands | 12 | ~90 | 80%+ |
| FileHandle | 2 | ~24 | 85%+ |
| Services | 1 | ~12 | 85%+ |
| **Total** | **15** | **~126** | **80%+** |

## 🧪 Test Data

### test.schema.xml
Contains the following test data:
- **Tables:** Users, Orders, OrderDetails, Products, UserRoles, vw_UserOrders (view)
- **Columns:** Various data types (int, nvarchar, decimal, datetime)
- **Foreign Keys:** 
  - Orders.UserId → Users.UserId
  - OrderDetails.OrderId → Orders.OrderId
  - OrderDetails.ProductId → Products.ProductId
  - UserRoles.UserId → Users.UserId
- **Stored Procedures:** sp_GetUserOrders, sp_CreateOrder, sp_UpdateOrderStatus, sp_GetUserById, sp_DeleteUser, usp_CalculateTotal

## 🛠️ Test Helper Classes

### TestDisplay
Mocks the `IDisplay` interface, capturing all display messages for test verification.

```csharp
var display = new TestDisplay();
// ... run command
Assert.True(display.ContainsInfo("expected message"));
Assert.True(display.ContainsError("error message"));
```

### TestFileHelper
Provides file operation helpers:
- `GetTestDataPath()` - Get test data path
- `GetTempPath()` - Get temporary directory
- `CreateTestSchemaFile()` - Copy test schema file
- `CleanupTempFiles()` - Clean up temporary files

### MockDatabase
Provides `IDatabase` mock implementation with default test data.

```csharp
var mockDb = MockDatabase.CreateTestData();
// Contains Users, Orders, Products tables with columns and SPs
```

## 🎯 Test Principles

### 1. AAA Pattern
All tests follow the **Arrange-Act-Assert** pattern:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Prepare test data
    var command = new SomeCommand(...);
    
    // Act - Execute action
    await command.RunAsync(args);
    
    // Assert - Verify results
    Assert.True(condition);
}
```

### 2. Test Naming
Use clear test names: `MethodName_Scenario_ExpectedResult`

### 3. Isolation
Each test is independent, using `IDisposable` to clean up temporary data:
```csharp
public void Dispose()
{
    TestFileHelper.CleanupTempFiles();
}
```

### 4. Cover Edge Cases
- ✅ Normal cases
- ✅ Null/empty values
- ✅ Invalid parameters
- ✅ Data not found
- ✅ Case insensitive

## 🔍 Key Test Points

### Partial String Search
Ensure these commands support **partial string search**:
- `FindTableCommand` (ft)
- `FindColumnCommand` (fc)
- `FindTableColumnCommand` (ftc)
- `FindStoredProcedureCommand` (fsp)

### Exact Match
`ShowAllColumnsCommand` (sc) uses **exact match**, not partial strings.

### XML Nested Structure
Verify correct XML format:
```xml
<Databases>
  <Database>
    <Tables>
      <Table>
        <Columns>
          <Column>
```

### Foreign Key Information
Ensure Foreign Key information is fully stored and retrieved.

## 📈 Continuous Improvement

### Areas for Improvement
- [ ] Add integration tests
- [ ] Performance tests (large schemas)
- [ ] Stress tests (concurrent operations)
- [ ] More edge case tests

### Known Limitations
- ERD Command UI portion cannot be unit tested (requires Terminal.Gui environment)
- Some file system operations may behave differently on different OSes

## 🤝 Contributing

When adding new tests, ensure:
1. Follow AAA pattern
2. Use clear naming
3. Clean up temporary data (IDisposable)
4. Cover normal and exceptional cases
5. Maintain 80%+ coverage

## 📄 License

Same as the main project.
