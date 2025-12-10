# Exit Codes - LogRotate for Windows

LogRotate for Windows uses standard exit codes to indicate the result of execution. These codes allow scripts, scheduled tasks, and automation tools to determine whether the operation succeeded or failed, and what type of error occurred.

## Exit Code Reference

| Exit Code | Constant Name | Description | When Used |
|-----------|---------------|-------------|-----------|
| 0 | `EXIT_SUCCESS` | Successful execution | Normal completion, help/usage displayed, or rotation completed successfully |
| 1 | `EXIT_GENERAL_ERROR` | General runtime error | Unexpected exceptions or runtime errors during execution |
| 2 | `EXIT_INVALID_ARGUMENTS` | Invalid command line arguments | Unknown or malformed command line options |
| 3 | `EXIT_CONFIG_ERROR` | Configuration file error | Config file not found or include directive path invalid |
| 4 | `EXIT_NO_FILES_TO_ROTATE` | No files to rotate | No log files found matching the configuration |

## Usage Examples

### PowerShell

```powershell
# Basic usage with exit code checking
logrotate C:\logs\logrotate.conf

switch ($LASTEXITCODE) {
    0 { Write-Host "✓ Rotation completed successfully" }
    1 { Write-Error "✗ General error occurred"; exit 1 }
    2 { Write-Error "✗ Invalid arguments"; exit 2 }
    3 { Write-Error "✗ Configuration file error"; exit 3 }
    4 { Write-Warning "⚠ No files to rotate"; exit 0 }
    default { Write-Error "✗ Unknown error: $LASTEXITCODE"; exit $LASTEXITCODE }
}
```

### Batch File

```batch
@echo off
logrotate C:\logs\logrotate.conf

if %ERRORLEVEL% EQU 0 (
    echo Rotation completed successfully
    exit /b 0
)

if %ERRORLEVEL% EQU 1 (
    echo General error occurred
    exit /b 1
)

if %ERRORLEVEL% EQU 2 (
    echo Invalid command line arguments
    exit /b 2
)

if %ERRORLEVEL% EQU 3 (
    echo Configuration file error
    exit /b 3
)

if %ERRORLEVEL% EQU 4 (
    echo No files to rotate
    exit /b 0
)

echo Unknown error: %ERRORLEVEL%
exit /b %ERRORLEVEL%
```

### Task Scheduler

When scheduling LogRotate in Windows Task Scheduler:

1. Create a wrapper script (PowerShell or Batch) that handles exit codes
2. Configure the task to:
   - Run whether user is logged on or not
   - Run with highest privileges (if needed for log file access)
   - Configure email alerts based on exit code

**Example Task Scheduler PowerShell wrapper:**

```powershell
# logrotate-scheduled.ps1
$logFile = "C:\logs\logrotate-task.log"
$configFile = "C:\logs\logrotate.conf"

"[$(Get-Date)] Starting log rotation..." | Out-File $logFile -Append

& logrotate $configFile

$exitCode = $LASTEXITCODE
"[$(Get-Date)] Exit code: $exitCode" | Out-File $logFile -Append

# Only fail the task for actual errors (not for "no files to rotate")
if ($exitCode -gt 0 -and $exitCode -ne 4) {
    "[$(Get-Date)] ERROR: Log rotation failed" | Out-File $logFile -Append
    exit $exitCode
}

"[$(Get-Date)] Log rotation completed" | Out-File $logFile -Append
exit 0
```

### CI/CD Pipelines

**GitHub Actions:**

```yaml
- name: Rotate logs
  run: |
    logrotate config.conf
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
      Write-Host "::notice::Logs rotated successfully"
    } elseif ($exitCode -eq 4) {
      Write-Host "::warning::No files to rotate"
      exit 0  # Don't fail the build
    } else {
      Write-Host "::error::Log rotation failed with exit code $exitCode"
      exit $exitCode
    }
  shell: pwsh
```

**Azure DevOps:**

```yaml
- task: PowerShell@2
  displayName: 'Rotate Logs'
  inputs:
    targetType: 'inline'
    script: |
      logrotate $(Build.SourcesDirectory)\config.conf

      if ($LASTEXITCODE -eq 0) {
        Write-Host "##vso[task.complete result=Succeeded;]Logs rotated"
      } elseif ($LASTEXITCODE -eq 4) {
        Write-Host "##vso[task.logissue type=warning]No files to rotate"
        exit 0
      } else {
        Write-Host "##vso[task.logissue type=error]Rotation failed: $LASTEXITCODE"
        exit $LASTEXITCODE
      }
```

## Best Practices

1. **Always check exit codes** in automated scripts and scheduled tasks
2. **Treat exit code 4** (no files to rotate) as a warning, not an error
3. **Log exit codes** for troubleshooting and auditing
4. **Use appropriate actions** based on the exit code:
   - Code 0: Continue normally
   - Code 1: Alert administrators, retry may help
   - Code 2: Fix command line arguments
   - Code 3: Check configuration file path and contents
   - Code 4: Normal condition, may indicate no logs were generated

## Testing Exit Codes

You can test exit codes manually:

```powershell
# Test success (show help)
logrotate --help
Write-Host "Exit code: $LASTEXITCODE"  # Should be 0

# Test invalid argument
logrotate --invalid-option
Write-Host "Exit code: $LASTEXITCODE"  # Should be 2

# Test missing config
logrotate nonexistent.conf
Write-Host "Exit code: $LASTEXITCODE"  # Should be 3 or 4
```

## Implementation Details

Exit codes are defined as constants in the `Program` class (Program.cs):

```csharp
internal const int EXIT_SUCCESS = 0;              // Successful execution
internal const int EXIT_GENERAL_ERROR = 1;        // General error
internal const int EXIT_INVALID_ARGUMENTS = 2;    // Invalid arguments
internal const int EXIT_CONFIG_ERROR = 3;         // Config file error
internal const int EXIT_NO_FILES_TO_ROTATE = 4;   // No files found
```

These constants ensure consistency across the codebase and make it easy to update exit code values if needed.
