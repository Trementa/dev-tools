# dev-tools
Repository with various tools which might or might not be of use.
Mostly targeted for developers.

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
1) Open PowerShell

2) Clone repository
git clone git@github.com:Trementa/dev-tools.git

3) Go to script directory
cd dev-tools\src\DevTools

4) Run script and optionally specify where your root is, e.g. D:\Source\MyCodeFolder
 .\Dev-Tools.ps1 [root]

5) Enter "helpme" to show a list of available commands.
If "helpme" isn't recognised by PowerShell you might have to close and re-open your console.


### Example output from "helmpe"
Trementa Development Tools version 3.1.1
Powershell Core 7.2.7
DevEnv: Visual Studio Community 2022, VisualStudioPreview/17.5.0-pre.1.0+33103.201 (latest detected version)

Command               Shorthand   Description
-------               ---------   -----------
Clean-Solution        cln         Clean solution structure from artifacts
Clr-Sln                           Clean solutions cache for repository
DevEnv                            Open latest version of Visual Studio installed
Edit-Profile                      Open all profile (.ps1) scripts
Edit-Solution         sln         List solution files below the current folder, open selected
Git-Diff              gd          List the latest commits, open difftool in folder mode for selected
Get-CommandDefinition which       Show location of command
Get-GitRepositories               List Git repositories
Get-PublicIP          whereami    Shows your public IP
MSBuild                           Shortcut to msbuild
Notepad               edit        Opens preferred text editor
Open-WindowsExplorer  we          Open Windows Explorer in the current directory
Reload                            Reload scripts
Remove-ProfileLoader              Remove profile loader
Run-Elevated          admin, sudo Launch terminal in Admin-mode
Set-ProfileLoader                 Install environment profile loader script
Sign-File                         Sign file with private key
Start-Docker                      Start Docker
Stop-Terminal         quit        Stop terminal process (close window)
Test-All                          Run all tests from current directory
Unblock                           Unblocks all file in current or specified folder with subfolders
Get-ToolsHelp         helpme      Show this list again

### Define folder short-cuts
The devenv.config file contains short-cuts for folders.
If there is a "devenv.config" file somewhere in the path from the root dev-tools automatically enables those definitions.

### Example devenv.config
{
	"folders":
	{
		"root": ".",
		"source": "..",
		"scripts": "dev-tools\\src",
		"quiz": "Quiz",
		"scraper" : "Scraper",
		"seb": "SEB-Scraper",
		"todo": "ToDo",
		"trementa": "Trementa",
		"zonsec": "Zonsec",
		"nuget": "nuget",		
		"bin": "**bin",
		"debug": "#(.*bin\\.*Debug.*?)",
		"release": "#(.*bin\\.*Debug.*?)"
	},	
	"visualstudio": "^18.0.0"
}

Each line defines a short-cut. Wildcards and regular expressions are allowed.
