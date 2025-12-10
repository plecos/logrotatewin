# LogRotate for Windows - Testing Documentation

## Overview

This document describes the testing strategy and implementation for LogRotate for Windows.

## Test Project Setup

- **Framework**: xUnit 2.6.2
- **Target**: .NET Framework 4.8
- **Assertion Library**: FluentAssertions 6.12.0
- **Mocking Library**: Moq 4.20.70
- **Code Coverage**: Coverlet

## Current Test Status

### Unit Tests Implemented

#### ✅ CmdLineArgsTests (15 tests - ALL PASSING)
Tests command-line argument parsing:
- Debug flag handling
- Force flag handling
- Verbose flag handling
- State file configuration
- Usage/Help flags
- Multiple config paths
- Combined flags
- Long and short flag formats

**Status**: ✅ **15/15 passing**

#### ⚠️ LogRotateStatusTests (10 tests - 4 passing, 6 failing)
Tests state file management:
- ✅ Setting rotation dates
- ✅ Persisting data across instances
- ✅ Creating missing state files
- ⚠️ Getting rotation dates (returns 1970-01-01 instead of DateTime.MinValue)
- ⚠️ Multiple file tracking (timing precision issues)
- ⚠️ Null/empty path handling (throws NullReferenceException)

**Status**: ⚠️ **4/10 passing** (Known issues with implementation - returns Unix epoch instead of DateTime.MinValue)

#### ✅ LoggingTests (11 tests - 10 passing, 1 failing)
Tests logging functionality:
- ✅ Debug mode logging
- ✅ Verbose mode logging
- ✅ Required and Error type messages
- ✅ Null and empty message handling
- ⚠️ Null exception handling (throws NullReferenceException)

**Status**: ✅ **10/11 passing**

#### ⚠️ ExitCodeTests (7 tests - ALL FAILING)
Tests exit code behavior:
- ⚠️ All tests fail due to exe path resolution issues
- Tests are correctly written but need exe to be built first

**Status**: ⚠️ **0/7 passing** (Path resolution issue - tests are correct)

### Summary

**Total Tests**: 40
**Passing**: 26 (65%)
**Failing**: 14 (35%)

**By Category**:
- ✅ Command-line parsing: 15/15 (100%)
- ✅ Logging: 10/11 (91%)
- ⚠️ State management: 4/10 (40%)
- ⚠️ Exit codes: 0/7 (0% - requires exe build)

## Known Issues

### 1. DateTime.MinValue vs Unix Epoch
The `logrotatestatus` class returns `new DateTime(1970, 1, 1)` instead of `DateTime.MinValue` for new files. This is an implementation detail that could be updated or the tests adjusted.

### 2. Null Reference Handling
Some classes don't handle null inputs gracefully:
- `logrotatestatus.GetRotationDate(null)` throws `NullReferenceException`
- `Logging.LogException(null)` throws `NullReferenceException`

### 3. Exit Code Test Exe Path
The `ExitCodeTests` can't locate the logrotate.exe. The path resolution logic needs to be updated to properly locate the executable relative to the test assembly.

## Test Helpers

### TestHelpers Class
Provides utilities for all tests:
- `CreateTempLogFile(long sizeInBytes)` - Creates test log files
- `CreateTempConfigFile(string content)` - Creates test config files
- `CreateTempDirectory()` - Creates temporary test directories
- `CleanupPath(string path)` - Safely cleans up test files/directories
- `IsFileCompressed(string path)` - Checks if file is gzip compressed
- `GetFileSize(string path)` - Gets file size in bytes
- `CountFiles(string directory, string pattern)` - Counts matching files

## Running Tests

### Run All Unit Tests
```powershell
dotnet test --filter "Category=Unit"
```

### Run Specific Test Class
```powershell
dotnet test --filter "FullyQualifiedName~CmdLineArgsTests"
```

### Run with Coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Run with Detailed Output
```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Future Test Implementation

### Integration Tests (Not Yet Implemented)
- RotationTests - File rotation scenarios
- CompressionTests - Gzip compression
- ScriptExecutionTests - Pre/post rotation scripts
- EmailTests - Email functionality
- ConfigParsingTests - Config file parsing

### End-to-End Tests (Not Yet Implemented)
- DailyRotationScenarioTests
- SizeBasedRotationScenarioTests
- ComplexConfigScenarioTests

### Performance Tests (Not Yet Implemented)
- Large file rotation (1GB+)
- Many file scenarios (1000+ files)
- Memory usage profiling

### Security Tests (Not Yet Implemented)
- Path traversal prevention
- Command injection prevention
- Symbolic link handling
- Secure file shredding

## CI/CD Integration

Tests can be integrated into GitHub Actions:

```yaml
- name: Run Unit Tests
  run: dotnet test --filter Category=Unit --logger trx

- name: Run Integration Tests
  run: dotnet test --filter Category=Integration --logger trx

- name: Generate Coverage Report
  run: dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

When adding new features:
1. Write unit tests first (TDD approach)
2. Ensure all existing tests pass
3. Add integration tests for complex scenarios
4. Update this documentation

## Test Coverage Goals

- **Unit Tests**: > 80% code coverage
- **Integration Tests**: All major workflows
- **E2E Tests**: Real-world scenarios
- **Performance Tests**: No regressions

## Notes

- All tests use `IDisposable` pattern for cleanup
- Tests are isolated and can run in parallel
- FluentAssertions provides readable test output
- `[Trait("Category", "Unit")]` allows filtering by test type
