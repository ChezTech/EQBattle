This script is used to create a new EQ folder for a box account.
Rather than copy all the files to a new folder (which sucks up a lot of space), this will make a symbolic folder link and override only the few specific files for the other character account.

Run like so:

```
Usage: .\NewCharFolder.ps1 -CharName YourCharName -ServerName test -EQFolder "C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest"
-CharName [required]
-ServerName [optional] Defaults to "test"
-EQFolder [optional] "C:\Users\Public\Daybreak Game Company\Installed Games\EverQuest"
```

* Open PowerShell window as Admin
* Run `.\NewCharFolder.ps1 Gandalf`
