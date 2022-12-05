# Trementa dev-tools
Opinionated developer tools repository.\
Mainly targeted for developers.\
For developers using .Net...with Visual Studio.

## PowerShell CmdLets
src\Cmdlets

### DisplayResolutionCmdlet
Change the display resolution.

### FileLockProcessCmdlet

### OpenApiGeneratorCmdlet
Generates a C# proxy given an OpenApi definition, i.e. swagger file as of old.

### SemVerCmdlet
Enables semver rules.


## Powershell Developer Tools
Collection of various functions for PowerShell.

### How to install the Powershell Developer Tools
1. Open PowerShell

2. Clone repository
 ```pwsh
 git clone git@github.com:Trementa/dev-tools.git
 ```

3. Go to script directory
 ```pwsh
 cd dev-tools\src\DevTools
 ```

4. Run script and optionally specify where your root is, e.g. D:\Source\MyCodeFolder
 ```pwsh
 .\Dev-Tools.ps1 [D:\Source\MyCodeFolder]
 ```

5. Enter "helpme" to show a list of available commands.\
 If "helpme" isn't recognised by PowerShell you might have to close and re-open your console.


### Functions provided

|Command               |Shorthand   |Description                                                         |
|----------------------|------------|--------------------------------------------------------------------|
|Clean-Solution        |cln         |Clean solution structure from artifacts                             |
|Clr-Sln               |            |Clean solutions cache for repository                                |
|DevEnv                |            |Open latest version of Visual Studio installed                      |
|Edit-Profile          |            |Open all profile (.ps1) scripts                                     |
|Edit-Solution         |sln         |List solution files below the current folder, open selected         |
|Git-Diff              |gd          |List the latest commits, open difftool in folder mode for selected  |
|Get-CommandDefinition |which       |Show location of command                                            |
|Get-GitRepositories   |            |List Git repositories                                               |
|Get-PublicIP          |whereami    |Shows your public IP                                                |
|MSBuild               |            |Shortcut to msbuild                                                 |
|Notepad               |edit        |Opens preferred text editor                                         |
|Open-WindowsExplorer  |we          |Open Windows Explorer in the current directory                      |
|Reload                |            |Reload scripts                                                      |
|Remove-ProfileLoader  |            |Remove profile loader                                               |
|Run-Elevated          |admin, sudo |Launch terminal in Admin-mode                                       |
|Set-ProfileLoader     |            |Install environment profile loader script                           |
|Sign-File             |            |Sign file with private key                                          |
|Start-Docker          |            |Start Docker                                                        |
|Stop-Terminal         |quit        |Stop terminal process (close window)                                |
|Test-All              |            |Run all tests from current directory                                |
|Unblock               |            |Unblocks all file in current or specified folder with subfolders    |

### Define folder short-cuts
The  ```devenv.config ``` file defines shortcuts for folders.\
If this file is located somewhere in the path from the root dev-tools automatically enables those definitions.

### Example devenv.config
```json
{
	"folders":
	{
		"root": ".",
		"source": "..",
		"scripts": "dev-tools\\src",
		"nuget": "nuget",		
		"bin": "**bin",
		"debug": "#(.*bin\\.*Debug.*?)",
		"release": "#(.*bin\\.*Debug.*?)"
	}
}
```

Each line defines a short-cut.\
Wildcards and regular expressions are allowed.
