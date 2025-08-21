# ===============================
# Install-Watchdog.ps1
# ===============================

# Configuration
$ServiceName = "WatchdogAgent"
$DisplayName = "Watchdog Agent"

# Automatically find the executable in the same folder as the script
$ExePath = Join-Path -Path $PSScriptRoot -ChildPath "Watchdog.Agent.exe"

# Check if the executable exists
if (-not (Test-Path $ExePath)) {
    Write-Error "Executable not found at path: $ExePath"
    exit 1
}

# Check if the service already exists
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($service -eq $null) {
    # Create the service using sc.exe (arguments must be separate)
    & sc.exe create $ServiceName binPath= "`"$ExePath`"" DisplayName= "`"$DisplayName`"" start= auto
    Write-Host "Service '$ServiceName' created successfully."
} else {
    Write-Host "Service '$ServiceName' already exists."
}

# Configure infinite automatic restart on failure (every failure restarts after 5 seconds)
sc.exe failure $ServiceName reset= 0 actions= restart/5000

Write-Host "Service recovery options set to restart on failure."

# Start the service
try {
    Start-Service -Name $ServiceName -ErrorAction Stop
    Write-Host "Service '$ServiceName' started successfully."
} catch {
    Write-Warning "Service '$ServiceName' could not be started. Error: $_"
}

