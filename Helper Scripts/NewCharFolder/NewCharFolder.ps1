
# Is there a registry entry for EQ?
$BaseEQFolder = "C:\Program Files (x86)\Steam\steamapps\common\Everquest F2P"
$BoxCharName = "Bob"

$BoxEQFolder = $BaseEQFolder + "-" + $BoxCharName
$BoxEQClient = Join-Path -Path $BaseEQFolder -ChildPath "eqclient.ini"
$BaseEQClient = Join-Path -Path $BoxEQFolder -ChildPath ($BoxCharName + "_eqclient.ini")

# Create the directory junction (hard links to all the files in the folder)
$dirLink = New-Item -ItemType Directory -Path $BoxEQFolder -Target $BaseEQFolder


# Now, override the couple of files we need to
# This is 'eqclient.ini' for each client
# we want to make a link from 'eqclient.ini' in the box folder to '<CharName>_eqclient.ini' in the main EQ folder
$fileLink = New-Item -ItemType SymbolicLink -Path $BoxEQClient -Target $BaseEQClient


$dirLink | Select-Object LinkType, Target
$fileLink | Select-Object LinkType, Target

# FileId is the hard link ID that the filenames point to
# use 'fsutil file queryfileid' or Get-SMBopenFile
# maybe: Get-ItemProperty C:\Test\Weather.xls | Format-List
