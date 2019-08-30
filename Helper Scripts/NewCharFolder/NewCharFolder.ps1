
# Is there a registry entry for EQ?
$BaseEQFolder = "C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P"
#$BaseEQFolder = "C:\Program Files (x86)\Steam\steamapps\common\EQSource"
$BoxCharName = "Balymoor"
$BoxEQFolder = $BaseEQFolder + "-" + $BoxCharName

# Create the folder
$null = New-Item -ItemType Directory -Path $BoxEQFolder

# Make sym links for each file and folder
$EQFiles = Get-ChildItem -Path $BaseEQFolder
foreach ($file in $EQFiles)
{
    $boxFilePath = Join-Path -Path $BoxEQFolder -ChildPath $file.Name
    if (!(Test-Path -Path $boxFilePath))
    {
        $null = New-Item -ItemType SymbolicLink -Path $boxFilePath -Target $file.FullName
    }
}

# Now, override the 'eqclient.ini' file to symlink to the named version in the base folder
$BoxEQClient = Join-Path -Path $BoxEQFolder -ChildPath "eqclient.ini"
$BaseEQClient = Join-Path -Path $BaseEQFolder -ChildPath "eqclient.ini"
$BaseCharEQClient = Join-Path -Path $BaseEQFolder -ChildPath ($BoxCharName + "_eqclient.ini")

# First, make sure the named character's 'eqclient.ini' file exists
if (!(Test-Path -Path $BaseCharEQClient))
{
    Copy-Item -Path $BaseEQClient -Destination $BaseCharEQClient
}

# Remove the new folder's 'eqclient.ini' that we symlinked above to the base 'eqclient.ini'
Remove-Item -Path $BoxEQClient

# Now, make the symlink from the new folder's 'eqclient.ini' to the base folder's named 'eqclient.ini'
$null = New-Item -ItemType SymbolicLink -Path $BoxEQClient -Target $BaseCharEQClient

# All done
