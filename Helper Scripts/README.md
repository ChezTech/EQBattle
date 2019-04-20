
1. Modify the `MoveRun.ps1` script to point to your locations for EQ and its log file folder
1. Create a shortcut *(one is supplied in this folder, verify it's properties for your file locations)*
   1. Target: `%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy Bypass -File MoveRun.ps1`
   1. Start in: `"C:\Repos\GitHub\EQBattle\Helper Scripts"` (or wherever this folder is)
   1. Change Icon: Browse to the `Everquest.ico` file in the EQ folder
   1. Run: Minimized
1. Drag shortcut to taskbar to pin it there for a quick launch