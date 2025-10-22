# ===============================
# Install-Watchdog.ps1
# ===============================

# Check and set execution policy if needed
$currentPolicy = Get-ExecutionPolicy -Scope CurrentUser
if ($currentPolicy -eq "Restricted") {
    Write-Host "Setting execution policy to RemoteSigned for current user..."
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
}

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

if ($service -ne $null) {
    Write-Host "Service '$ServiceName' already exists. Stopping and removing it..."
    
    # Stop the service if it's running
    if ($service.Status -eq "Running") {
        try {
            Stop-Service -Name $ServiceName -Force -ErrorAction Stop
            Write-Host "Service '$ServiceName' stopped successfully."
            Start-Sleep -Seconds 2  # Give it time to stop
        } catch {
            Write-Warning "Could not stop service '$ServiceName'. Error: $_"
        }
    }
    
    # Delete the existing service
    try {
        & sc.exe delete $ServiceName
        Write-Host "Service '$ServiceName' deleted successfully."
        Start-Sleep -Seconds 1  # Give it time to delete
    } catch {
        Write-Warning "Could not delete service '$ServiceName'. Error: $_"
    }
}

# Create the service using sc.exe (arguments must be separate)
& sc.exe create $ServiceName binPath= "`"$ExePath`"" DisplayName= "`"$DisplayName`"" start= auto
Write-Host "Service '$ServiceName' created successfully."

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

