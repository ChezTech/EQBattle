# This works, but ugly message (need the '#' at the very first position)
# #Requires -RunAsAdministrator

# First time on a new PC, you'll need to allow scripts to run
# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.security/set-executionpolicy?view=powershell-7
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
# Get-ExecutionPolicy -List

# Is there a registry entry for EQ?

# ============================================================================== User Input
$BaseEQFolder = "C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest"
$BoxCharName = "Aiyya"

# Name of the server, for writing the UI .ini files
$ServerName = "test"

# ============================================================================== Code

function Test-Administrator  
{  
    [OutputType([bool])]
    param()
    process {
        [Security.Principal.WindowsPrincipal]$user = [Security.Principal.WindowsIdentity]::GetCurrent();
        return $user.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator);
    }
}

if(-not (Test-Administrator))
{
	$Message = "You must run this script as an Administrator"
	
	# While this is the correct way, it has an ugly output format w/ a stack trace.
	#Write-Error -Message $Message -Category PermissionDenied
	
	# This is more friendly
	Write-Host $Message -ForegroundColor Red -BackgroundColor Black
    exit
}

# -------------------------------------------------
# Define variables

$BoxEQFolder = $BaseEQFolder + "-" + $BoxCharName
# C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest-Aiyya

$BoxEQClient = Join-Path -Path $BoxEQFolder -ChildPath "eqclient.ini"
# C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest-Aiyya\eqclient.ini

$BaseEQClient = Join-Path -Path $BaseEQFolder -ChildPath "eqclient.ini"
# C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest\eqclient.ini

$BaseCharEQClient = Join-Path -Path $BaseEQFolder -ChildPath ($BoxCharName + "_eqclient.ini")
# C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest\Aiyya_eqclient.ini

$BaseCharServerIni = Join-Path -Path $BaseEQFolder -ChildPath ($BoxCharName + "_" + $ServerName + ".ini")
# C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest\Aiyya_test.ini

$UIBaseCharServerIni = Join-Path -Path $BaseEQFolder -ChildPath ("UI_" + $BoxCharName + "_" + $ServerName + ".ini")
# C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest\UI_Aiyya_test.ini


# -------------------------------------------------
# Make sure the New Character's .ini files exist in the original folder

# First, make sure the named character's 'eqclient.ini' file exists, copy the base 'eqclient.ini' if not.
if (!(Test-Path -Path $BaseCharEQClient))
{
    Copy-Item -Path $BaseEQClient -Destination $BaseCharEQClient
}

# Now, make sure the other UI files exists. Create them if not. They will get sym-linked below

# '<CharName>_<ServerName>.ini'
if (!(Test-Path -Path $BaseCharServerIni))
{
	New-Item -ItemType File -Path $BaseCharServerIni
}

# 'UI_<CharName>_<ServerName>.ini'
if (!(Test-Path -Path $UIBaseCharServerIni))
{
	New-Item -ItemType File -Path $UIBaseCharServerIni
}


# -------------------------------------------------
# Create the folder
if (!(Test-Path -Path $BoxEQFolder))
{
    $null = New-Item -ItemType Directory -Path $BoxEQFolder
}

# -------------------------------------------------
# Make sym links for each file and folder

$HardExtCollection = ".exe", ".dll"

$EQFiles = Get-ChildItem -Path $BaseEQFolder
foreach ($file in $EQFiles)
{
    $boxFilePath = Join-Path -Path $BoxEQFolder -ChildPath $file.Name
    if (!(Test-Path -Path $boxFilePath))
    {
        if ($file.Attributes -eq "Directory")
        {
            $linkType = "SymbolicLink"
        }
        ElseIf ($HardExtCollection -Contains $file.Extension)
        {
            $linkType = "HardLink"
        }
        Else 
        {
            $linkType = "SymbolicLink"
        }
        
        $null = New-Item -ItemType $linkType -Path $boxFilePath -Target $file.FullName
    }
}


# -------------------------------------------------
# Now, override the 'eqclient.ini' file to symlink to the named version in the base folder

# Remove the new folder's 'eqclient.ini' that we symlinked above to the base 'eqclient.ini'
if (Test-Path -Path $BoxEQClient)
{
    Remove-Item -Path $BoxEQClient
}

# Now, make the symlink from the new folder's 'eqclient.ini' to the base folder's named 'eqclient.ini'
$null = New-Item -ItemType SymbolicLink -Path $BoxEQClient -Target $BaseCharEQClient

# Verify
Get-Item $BoxEQClient | Select-Object Name,LinkType,Target

# All done
