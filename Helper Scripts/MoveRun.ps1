# Used to move a log file then start EQ
# Ensures log files are moved if too big or too old so that parsing can be done on a reasonably sized file

# ==============================================================================================
# Usage: .\MoveRun.ps1 -EQFolder "C:\Program Files\Sony\EverQuest Trilogy" -MaxFileSize 5MB -MaxFileAgeDays 7
# ==============================================================================================

Param (
    [string] $EQFolder = "C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P",
    # [string] $EQFolder = "C:\Program Files\Sony\EverQuest Trilogy",
    $MaxFileSize = 5MB,
    $MaxFileAgeDays = 7
)

# ==============================================================================================
# Change these values as needed
# ==============================================================================================

$EQPath = Join-Path -Path $EQFolder -ChildPath "LaunchPad.exe"

# Log file location
$LogFileLocation = Join-Path -Path $EQFolder -ChildPath "Logs"

# Destination log file path
# - Blank means to use the same location as the source log file
# - Relative paths need to start with a "." e.g. "./LogBackup" or "../LogBackup"
$LogFileDestination = ""

# File regex pattern for log files (to exclude log files that have the date added to them)
$LogFilePattern = "^eqlog\D*\.txt"

# Date format string
$DateFormat = "yyyy-MM-dd-HHmmss"

# ==============================================================================================
# This is the code that moves the log file, if needed, then starts up EQ
# ==============================================================================================

$date = Get-Date
$dateStr = $date.ToString($DateFormat)
$time = $date.AddDays(-$MaxFileAgeDays)
$size = $MaxFileSize

$newPath = If ($LogFileDestination -eq "" -or $LogFileDestination.StartsWith(".")) {Join-Path -Path $LogFileLocation -ChildPath $LogFileDestination} Else {$LogFileDestination}
$newPath = Resolve-Path -Path $newPath

# Get files that match the name pattern, that are bigger than our size limit or older than our age limit
Get-ChildItem $LogFileLocation `
    | Where-Object {$_.Name -match $LogFilePattern -and ($_.Length -gt $size -or $_.LastWriteTime -lt $time)} `
    | ForEach-Object {$_.MoveTo($(Join-Path -Path $newPath -ChildPath $($_.BaseName + "_" + $dateStr + $_.Extension)))}

# I don't know why this doesn't work. It results in a name of just the $dateStr, no BaseName, no Extension
#    | Move-Item -Destination $(Join-Path -Path $newPath -ChildPath $(BaseName + "_" + $dateStr + Extension))


# ------------------- Now start up EQ
Start-Process -FilePath $EQPath -WorkingDirectory $(Get-ItemProperty -Path $EQPath).Directory

# Profit!
