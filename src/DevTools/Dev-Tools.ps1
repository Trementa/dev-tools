<#
.Synopsis
	Custom general profile with added functionality for development.
.Description
	Multiple helper tools for different development tasks.
	Forked from https://github.com/Trementa/dev-tools
	The license permits any use.
	
.License
	MIT License

	Copyright (c) 2022 Trementa AB

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
#>

param(
	[string] $defaultRoot,
	[Switch] $createEnvJson,
	[Switch] $install,
	[Switch] $continue,
	[Switch] $exit
)
$defaultRoot = if($defaultRoot){resolve-path $defaultRoot}else{get-location}

<#
.SYNOPSIS
	Use default process run arguments and retrieve path to executable for running process using the rules:
	1) If it is running in the context of Windows Terminal use that as the executable without arguments.
	2) If not in a Windows Terminal context just return the path to the executable of the current process.
#>
function Get-TerminalPath {
	# Running in Windows Terminal?
	$process = (get-process -id $pid).Parent
	do {
		if($process.Name -eq 'windowsterminal' -and (test-path (get-command 'wt.exe').Path)) {
			return @{Exe = "wt.exe"; Args = @() }
		}
		$process = $process.Parent
	} while($process)
	
	# Not windows terminal, use current process
	return Get-PowershellPath
}

function Get-PowershellPath {
	$cd = (Get-Location).Path
	return @{Exe = (get-process -id $pid).Path; Args = "-NoExit", "-Command", "cd $($cd)"}
}

<#
.SYNOPSIS
	Stop the process running the parent container.
	In effect closes the current window.
#>
function Stop-Terminal {
	# Running in Windows Terminal?
	$pwshProcess = get-process -id $pid
	$process = $pwshProcess
	do {
		$process = $process.Parent
		if($process.Name -eq 'windowsterminal') {
			stop-process $process
		}
		if($process.Name -eq $pwshProcess.Name) {
			$pwshProcess = $process
		}
	} while($process)
	
	stop-process $pwshProcess
}

<#
.SYNOPSIS
	Launch terminal elevated (administrator mode)
#>
function Run-Elevated {
	$pwsh = Get-TerminalPath
	Start-Process $pwsh.Exe -Verb runAs -ArgumentList $pwsh.Args
}

<#
.SYNOPSIS
	Launch new terminal instance
#>
function New-terminal {
	$pwsh = Get-TerminalPath
	Start-Process $pwsh.Exe -ArgumentList $pwsh.Args
}

<#
.SYNOPSIS
	Reload profile environment
	Invoke-Command { & "powershell.exe" } -NoNewScope # PowerShell 5
	Invoke-Command { & "pwsh.exe"       } -NoNewScope # PowerShell 7
	Or a longer but more robust command, the following should work correctly across versions:

	Get-Process -Id $PID | Select-Object -ExpandProperty Path | ForEach-Object { Invoke-Command { & "$_" } -NoNewScope }
	You will, of course, lose all your variables etc. from the previous session.
#>
function Reload {
	$pwsh = Get-PowershellPath
	Invoke-Command { & "$($pwsh.Exe)" } -NoNewScope
}

<#
.SYNOPSIS
	Select folder from regex
#>
function Select-Directory {
	param([string] $path)

	$global:counter = 1;
	$cd = (Get-Location).Path.Length
	$directories = @(Get-ChildItem -Recurse -Directory |
		lkjl			 where {$_.FullName -match $path} |
					 Select-Object @{Name = "Id"; Expression = { $global:counter; $global:counter++ } }, Name, FullName, @{Name = "Path"; Expression = { $_.FullName.SubString($cd) } })

	if ($directories.Count -eq 0) {
		"No such path $($path)"
		return
	}

	if ($directories.Count -gt 1) {
		$directories | Format-Table -Property @{Label = "#"; Expression = { $_.Id } }, Path -AutoSize | Out-Host

		while($value -eq $NULL -or $value -gt $directories.Count -or $value -le 0) {
			$input = Read-Host "Select folder (1-$($directories.Count))"
			if (!$input) {
				return $null
			}
			$value = $input -as [Int]		
		}
	}
	else {
		$value = 1
	}
	push-location $($directories[$value - 1].FullName) -StackName devstack 						
}

<#
.SYNOPSIS
	Create local configuration from all devenv.config files in current directory up to the root directory.
	Sets directory labels and selects visual studio instance.
#>
function Apply-Configuration
{
	$cd = get-location
	try {
		function Get-ObjectMembers {
			[CmdletBinding()]
			Param(
				[Parameter(Mandatory = $True, ValueFromPipeline = $True)]
				[PSCustomObject]$obj
			)
			$obj | Get-Member -MemberType NoteProperty | ForEach-Object {
				$key = $_.Name
				[PSCustomObject]@{Key = $key; Value = $obj."$key" }
			}
		}

		function Get-DevEnvConfig {
			param([string] $cd)
			$labelDirMapping = @()
			$envJson = Join-Path $cd 'devenv.config'
			if (Test-Path $envJson) {
				push-location $cd
					$json = Get-Content $envJson | ConvertFrom-Json
					if($json.visualstudio) { $vsversion = $json.visualstudio }
					($json.folders) | Get-ObjectMembers | ForEach-Object {
						if($_.Value){
							if($_.Value.StartsWith("*"))
							{
								$labelDirMapping += @{$_.Key = $_.Value }
							}
							else
							{
								$absPath = Resolve-Path $_.Value
								if (Test-Path $absPath) {
									$labelDirMapping += @{$_.Key = $absPath.Path }
								}
							}
						}
					}
				pop-location
			}
			return @($labelDirMapping, $vsversion)
		}

		function Search-Configuration {
			param([string] $path)
			if($path) {
                ($mapP, $vsvP) = Search-Configuration(Split-Path $path -Parent)
                ($map, $vsv) = Get-DevEnvConfig($path)
	        }

            $vsv = if($vsp) { $vsp } else { $vsvP }
            return ($mapP + $map, $vsv)
        }

		($labelMap, $vsversion) = Search-Configuration(Get-Location)
		foreach($map in $labelMap) {
			$label = $map.Keys[0]
			$path = $map[$label]
			if($path.StartsWith("#"))
			{
				$path = $path.Substring(1)
				$query = "function global:set_$($label) { Select-Directory '$($path)'}"
			}
			elseif($path.StartsWith("*"))
			{
				$query = "function global:set_$($label) { `$cd = gci -recurse -Directory | where {`$_.FullName -like '$($path)'} | select -first 1; if(`$cd) { push-location `$cd -StackName devstack } else { ""No '$($path)' folder found"" }}"
			}
			else
			{
				$query = "function global:set_$($label) {Set-Location '$($path)'}"
			}
			Invoke-Expression $query
			if ($isModule) { Export-ModuleMember $label }
			Set-Alias -Name $label -Value "set_$($label)" -Scope Global
		}
        return ($labelMap, $vsversion)
	} catch{}
	finally {
		set-location $cd >$null
	}
}

<#
.SYNOPSIS
	Method that creates the window title
#>
function Get-WindowTitle {
	if ($isAdmin) {	$prefix = "[ADMIN] "}
	else { $prefix = ""	}

	try {
		$isGitRepository = git rev-parse --is-inside-work-tree

		if ($LastExitCode -ne 128 -and $isGitRepository) {
			$symbolicref = git symbolic-ref HEAD

			if ($NULL -ne $symbolicref) {
				$title += "'" + (Split-Path -Leaf (git remote get-url origin)) + "' "
				$symbolicref = $symbolicref.substring($symbolicref.LastIndexOf("/") + 1)
				$title += $symbolicref + ": "
				$status = git status --porcelain #--untracked-files=all
				
				if ( $status ) {
					$matches = [regex]::matches([system.string]::join("`n", $status), "(?m)^.{2}")
					$statusTotals = @{}
					foreach ( $match in $matches ) {
						if ( ![string]::IsNullOrEmpty($match.Value) ) {
							$matchValue = $match.Value.Replace(" ", "-")

							if ( !$statusTotals.ContainsKey($matchValue) ) {
								$statusTotals.Add($matchValue, 1)
							}
							else {
								$statusTotals.Set_Item($matchValue, $statusTotals.Get_Item($matchValue) + 1)
							}
						}
					}
					foreach ( $dictEntry in $statusTotals.GetEnumerator() | Sort-Object Name) {
						$title += [string]::format("{0}:{1} ", $dictEntry.Name, $dictEntry.Value)
					}
				}
				else {
					$title += "nothing to commit (working dir clean)"
				}
			}
			return $prefix + $title
		}
	} catch { return $prefix + " Error: $($_)" }
	return $prefix + "Not a Git repository"
}

<#
.SYNOPSIS
	Clear solution cache.
#>
function Clr-Sln
{
	Set-Variable -Name $script:CACHE_VARIABLE_NAME -Scope Global -Value @{}
}

<#
.SYNOPSIS
	Method that creates the prompt string
#>
function Get-Prompt
{
	# History ID
	$HistoryId = $MyInvocation.HistoryId
	# Uncomment below for leading zeros
	# $HistoryId = '{0:d4}' -f $MyInvocation.HistoryId
	# Write-Host -Object "$HistoryId`: " -NoNewline -ForegroundColor Cyan
 
	## Path
	$Drive = $pwd.Drive.Name
	$Pwds = $pwd -split "\\" | Where-Object { -Not [String]::IsNullOrEmpty($_) }
	$PwdPath = if ($Pwds.Count -gt 3) {
		$ParentFolder = Split-Path -Path (Split-Path -Path $pwd -Parent) -Leaf
		$CurrentFolder = Split-Path -Path $pwd -Leaf
		"..\$ParentFolder\$CurrentFolder"
	}
	elseif ($Pwds.Count -eq 3) {
		$ParentFolder = Split-Path -Path (Split-Path -Path $pwd -Parent) -Leaf
		$CurrentFolder = Split-Path -Path $pwd -Leaf
		"$ParentFolder\$CurrentFolder"
	}
	elseif ($Pwds.Count -eq 2) {
		Split-Path -Path $pwd -Leaf
	}
	else { "" }
 
	if ($isAdmin) {
		"$Drive`:\$PwdPath$ "
	}
	else {
		"$Drive`:\$PwdPath> "
	}
}

<#
.SYNOPSIS
	Default function name for custom Powershell prompt
#>
function prompt {
	<#
	.SYNOPSIS
		Set powershell window title.
	#>
	function Set-WindowTitle {
		$Host.UI.RawUI.WindowTitle = Get-WindowTitle
	}

	function Clear-StatusBar {
		$pos = $host.UI.RawUI.CursorPosition
		$maxY = $host.UI.RawUI.WindowSize.Height - 1
		$width = $host.UI.RawUI.WindowSize.Width
		$host.UI.RawUI.CursorPosition = @{ x = 0; y = $maxY }
		write-host -nonewline " ".PadRight($width, ' ')
		$host.UI.RawUI.CursorPosition = $pos		
	}
# https://powershellone.wordpress.com/2021/04/06/control-split-panes-in-windows-terminal-through-powershell/
	function Set-StatusBar {
		$pos = $host.UI.RawUI.CursorPosition
		$maxY = $host.UI.RawUI.WindowSize.Height - 1
		$width = $host.UI.RawUI.WindowSize.Width
		$host.UI.RawUI.CursorPosition = @{ x = 0; y = $maxY }
		$text = $Host.UI.RawUI.WindowTitle
		if($pos.y -eq $maxY - 10) { write-host ''; $pos.y = $pos.y - 11 }
		write-host -nonewline -foregroundcolor black -backgroundcolor green $text.PadRight($width, ' ')
		$host.UI.RawUI.CursorPosition = $pos
	}

	#Clear-StatusBar >$null
	Apply-Configuration >$null
	Set-WindowTitle >$null
	#Set-StatusBar >$null
	Get-Prompt
}

<#
.SYNOPSIS
	Return public IP for this device
#>
function Get-PublicIP
{
	@{
		IPv4 = (Invoke-RestMethod -uri http://ipv4.icanhazip.com);
		IPv6 = (Invoke-RestMethod -uri http://ipv6.icanhazip.com);
	}
}

<#
.SYNOPSIS
	Set path for your default editor.
	Configurable
#>
function Set-DefaultEditor {
	$npp = "$env:ProgramFiles\Notepad++\notepad++.exe"
	if (Test-Path $npp) {
		Set-Alias -Name notepad -Value $npp -Scope Global
	}
}

# {
# "name": "Developer PowerShell",
# "commandline": "powershell.exe -NoExit -Command \"&{ $vsInstallPath=& \"${env:ProgramFiles(x86)}/'Microsoft Visual Studio'/Installer/vswhere.exe\" -prerelease -latest -property installationPath; Import-Module \"$vsInstallPath/Common7/Tools/Microsoft.VisualStudio.DevShell.dll\"; Enter-VsDevShell -VsInstallPath $vsInstallPath -SkipAutomaticLocation }\"",
# "icon": "ms-appx:///ProfileIcons/{61c54bbd-c2c6-5271-96e7-009a87ff44bf}.png"
# }

<#
.SYNOPSIS
	Set default Visual Studio instance.
	Configurable
#>
function Set-Devenv {
	$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
	if (Test-Path $vswhere) {
		$devEnvDisplayName = & $vswhere -latest -prerelease -property displayName
		$devEnvInstallationName = & $vswhere -latest -prerelease -property installationName
		$devEnv = & $vswhere -latest -prerelease -property productPath

		Set-Alias -Name DevEnv -Value "$devEnv" -Scope Global
		"DevEnv: $devEnvDisplayName, $devEnvInstallationName (latest detected version)"
	}
	else {
		Write-Host "Visual Studio not installed" -ForegroundColor Red
	}
}

<#
.SYNOPSIS
	Show all solutions in folders recursively.
#>
function Select-Solution {
	param(
		[int] $value,
		[Switch] $force
	)

	function GetOrAdd-Cache {
		[cmdletbinding()]
		Param(
			[Parameter(Mandatory = $true)]
			$Key,

			[Parameter(Mandatory = $true)]
			[ScriptBlock]
			$ScriptBlock
		)

		if (-not (Get-Variable -Name $script:CACHE_VARIABLE_NAME -Scope Global -ErrorAction SilentlyContinue)) {
			Set-Variable -Name $script:CACHE_VARIABLE_NAME -Scope Global -Value @{}
		}

		$cache = Get-Variable -Name $script:CACHE_VARIABLE_NAME -Scope Global
		if (-not $cache.Value.ContainsKey($Key)) {
			$cachedValue = &$ScriptBlock
			$cache.Value[$Key] = $cachedValue
		}
		else {
			$cachedValue = $cache.Value[$Key]
		}

		$cachedValue
	}
	
	$solutions = GetOrAdd-Cache -Key (Get-Location).Path -ScriptBlock {
		$global:counter = 1;
		$cd = (Get-Location).Path.Length
		return @(Get-ChildItem *.sln -Recurse | Sort-Object -Property Name |
			Select-Object @{Name = "Id"; Expression = { $global:counter; $global:counter++ } }, Name, FullName, @{Name = "Path"; Expression = { $_.FullName.SubString($cd) } })
	}

	if ($solutions.Count -eq 0) {
		return ""
	}
	if ($solutions.Count -gt 1) {
		$solutions | Format-Table -Property @{Label = "#"; Expression = { $_.Id } }, Name, Path -AutoSize | Out-Host
		while($value -eq $NULL -or $value -gt $solutions.Count -or $value -le 0) {
			$input = Read-Host "Select solution (1-$($solutions.Count))"
			if (!$input) {
				return $null
			}
			$value = $input -as [Int]		
		}
	}
	else {
		$value = 1
	}
	return $($solutions[$value - 1].FullName)
}

<#
.SYNOPSIS
	Select solution and open with Visual Studio.
.PARAMETER
	Solution - specific sln-file or index-number
#>
function Edit-Solution {
	param
	(
		[string] $solution
	)

    $index = $solution -as [int]
	if (!$solution -or $index -gt 0) { $solution = Select-Solution($index) }
	if ($solution) {
		Write-Host "Opening solution $solution"
		DevEnv "$solution"
	}
	else {
		"Unknown solution"
	}
}

<#
.SYNOPSIS
	Open default powershell profile in editor.
#>
function Edit-Profile {
	if ($host.Name -match "ise") {
		$psISE.CurrentPowerShellTab.Files.Add($profile.CurrentUserAllHosts)
	}
	else {
		notepad "$PSCommandPath"
		notepad "$($profile.CurrentUserAllHosts)"
	}
}

<#
.SYNOPSIS
	Start docker desktop.
#>
function Start-Docker {
	Get-Process 'com.docker.proxy' >$null 2>&1
	if (-Not $?) {
		Docker-Desktop
		"Starting Docker"
		do {
			Write-Host "." -NoNewLine
			Sleep 1
			Get-Process 'com.docker.proxy' >$null 2>&1
		} while (-Not $?)
		""
	}
	"Docker running"
	
	function Start-Docker {
		$dockerDesktop = "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"
		if (Test-Path $dockerDesktop) {
			Get-Process 'com.docker.proxy' >$null 2>&1
			if ($?) {
				"Docker Desktop is running"
			}
			else {
				&$dockerDesktop
			}
		}
	}	
}

<#
.SYNOPSIS
	Remove ACL descriptors (file has been blocked because it was downloaded from...) 
#>
function Remove-ACL {    
	[CmdletBinding(SupportsShouldProcess = $True)]
	Param(
		[parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
		[ValidateNotNullOrEmpty()]
		[ValidateScript({ Test-Path $_ -PathType Container })]
		[String[]]$Folder,
		[Switch]$Recurse
	)

	Process {

		foreach ($f in $Folder) {

			if ($Recurse) { $Folders = $(Get-ChildItem $f -Recurse -Directory).FullName } else { $Folders = $f }

			if ($null -ne $Folders) {

				$Folders | ForEach-Object {

					# Remove inheritance
					$acl = Get-Acl $_
					$acl.SetAccessRuleProtection($true, $true)
					Set-Acl $_ $acl

					# Remove ACL
					$acl = Get-Acl $_
					$acl.Access | % { $acl.RemoveAccessRule($_) } | Out-Null

					# Add local admin
					$permission = "BUILTIN\Administrators", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
					$rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
					$acl.SetAccessRule($rule)

					Set-Acl $_ $acl

					Write-Verbose "Remove-HCacl: Inheritance disabled and permissions removed from $_"
				}
			}
			else {
				Write-Verbose "Remove-HCacl: No subfolders found for $f"
			}
		}
	}
}

function Test-All {
	$testProjects = (gci *Tests*csproj -Recurse)
	$testProjects | ForEach-Object { dotnet build $_ }
	$testAssemblies = $testProjects | foreach { Join-Path -Path (Join-Path -Path $_.DirectoryName -ChildPath 'bin\Debug\netcoreapp3.0') -ChildPath ([System.IO.Path]::GetFileNameWithoutExtension($_) + '.dll') }
	dotnet vstest $testAssemblies
}

function Git-Diff {
	$index = 1
	$loglines = & git log -20 --format=oneline
	$table = $loglines | % { New-Object psobject -Property @{Index = $index++; Id = $_.Split(' ', 2)[0]; Message = $_.Split(' ', 2)[1]; }; }

	$global:counter = 1;
	$table | Format-Table -Property Id, Index, Message  -AutoSize
	do {
		$input = Read-Host "Select checkin (1-$($table.Count)) (adding an additional index compares the selected pair)"
		if ($input -eq "x" -or $input -eq "exit") {
			return;
		}
		$values = $input.Split(' ', 2) | % { $_ -as [int] }
		$valid = $values[0] -ne $NULL -and $values[0] -le $table.Count -and $values[0] -gt 0
		if ($values.Length -gt 1) {
			$valid = $valid -and $values[1] -ne $NULL -and $values[1] -le $table.Count -and $values[1] -gt 0
			$valid = $valid -and $values[0] -ne $values[1]
		}
	} until($valid)
	
	
	if ($values.Length -gt 1) {
		Write-Host "Opening diff for $($values[0]) and $($values[1]) - $($table[$values[0] - 1].Message)"
		Start-Process "git" -ArgumentList("difftool", $($($table[$values[1] - 1].Id)), $($table[$values[0] - 1].Id), "--dir-diff") -WindowStyle Minimized
	}
	else {
		Write-Host "Opening diff for $($table[$values[0] - 1].Message)"
		Start-Process "git" -ArgumentList("difftool", $($($table[$values[0] - 1].Id) + "~"), $($table[$values[0] - 1].Id), "--dir-diff") -WindowStyle Minimized
	}
}

function Clean-Solution {
	param
	(
		[string] $solution
	)

	if (!$solution) { $solution = Select-Solution }
	if (!$solution) { return }
	
	$dir = Split-Path "$solution"
	"Cleaning $solution"
	Push-Location $dir
	$projects = Get-Content "$solution" |
	Select-String 'Project\(' |
	ForEach-Object {
		$projectParts = $_ -Split '[,=]' | ForEach-Object { $_.Trim('[ "{}]') };
		New-Object PSObject -Property @{
			Name = $projectParts[1];
			File = $projectParts[2];
			Guid = $projectParts[3]
		} 
	} | Where -Property File -like "*.*proj"
	 
	foreach ($project in $projects) {
		$path = Split-Path $project.File
		$bin = Join-Path -Path $path -ChildPath bin
		$obj = Join-Path -Path $path -ChildPath obj
		$debug = Join-Path -Path $path -ChildPath debug
		$release = Join-Path -Path $path -ChildPath release
		$bin, $obj, $debug, $release | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
	}
	Pop-Location
}

function Open-WindowsExplorer {
	Invoke-Item (get-location)
}

function Get-CommandDefinition {
	Get-Command $Args[0] | Select-Object -ExpandProperty Definition
}

function unblock {
	param
	(
		[string]$path = "."
	)

	Remove-ACL $path -Recurse
}

function Get-GitRepositories {
	Get-ChildItem . -Attributes Directory+Hidden -ErrorAction SilentlyContinue -Filter ".git" -Recurse | ForEach-Object { Write-Host $_.Parent.Name }
}

function Get-GitStatus {
	# Escape and colour codes for use in virtual terminal escape sequences
	$esc = [char]27
	$red = '31'
	$green = '32'

	# Get child folders of the current folder which contain a '.git' folder
	Get-ChildItem -Path . -Attributes Directory+Hidden -Recurse -Filter '.git' | 
	ForEach-Object { 
		# Assume the parent folder of this .git folder is the working copy
		$workingCopy = $_.Parent
		$repositoryName = $workingCopy.Name

		# Change the working folder to the working copy
		Push-Location $workingCopy.FullName

		# Update progress, as using -AutoSize on Format-Table
		# stops anything being written to the terminal until 
		# *all* processing is finished
		Write-Progress `
			-Activity 'Check For Local Changes' `
			-Status 'Checking:' `
			-CurrentOperation $repositoryName

		# Get a list of untracked/uncommitted changes
		[Array]$gitStatus = $(git status --porcelain) | 
			ForEach-Object { $_.Trim() }

		# Status includes VT escape sequences for coloured text
		$status = ($gitStatus) `
			? "$esc[$($red)mCHECK$esc[0m" `
			: "$esc[$($green)mOK$esc[0m"

		# For some reason, the git status --porcelain output returns 
		# two '?' chars for untracked changes, when all other statuses 
		# are one character... this just cleans it up so that it's 
		# nicer to scan visually in the terminal
		$details = ($gitStatus -replace '\?\?', '?' | Out-String).TrimEnd()

		# Change back to the original directory
		Pop-Location

		# Return a simple 'row' object containing all the info
		[PSCustomObject]@{ 
			Status = $status
			'Working Copy' = $repositoryName
			Details = $details
		}
	} |
	Format-Table -Wrap -AutoSize	
}

function Enable-dotnet-TabCompletion {
	Register-ArgumentCompleter -Native -CommandName dotnet -ScriptBlock {
     param($commandName, $wordToComplete, $cursorPosition)
         dotnet complete --position $cursorPosition "$wordToComplete" | ForEach-Object {
            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
         }
	}
}

function Get-Commands {
	[PsObject[]]$HelpArray = @()
	$HelpArray += [pscustomobject]@{ Command = "Clean-Solution"  			; Shorthand = "cln"; Description = "Clean solution structure from artifacts" }
	$HelpArray += [pscustomobject]@{ Command = "Clr-Sln"  			        ; Description = "Clean solutions cache for repository" }
	$HelpArray += [pscustomobject]@{ Command = "DevEnv"          			; Description = "Open latest version of Visual Studio installed" }
	$HelpArray += [pscustomobject]@{ Command = "Edit-Profile"    			; Description = "Open all profile (.ps1) scripts" }
	$HelpArray += [pscustomobject]@{ Command = "Edit-Solution"   			; Shorthand = "sln"; Description = "List solution files below the current folder, open selected" }
	$HelpArray += [pscustomobject]@{ Command = "Git-Diff"        			; Shorthand = "gd"; Description = "List the latest commits, open difftool in folder mode for selected" }
	$HelpArray += [pscustomobject]@{ Command = "Get-CommandDefinition"		; Shorthand = "which"; Description = "Show location of command" }
	$HelpArray += [pscustomobject]@{ Command = "Get-GitRepositories"		; Description = "List Git repositories" }
	$HelpArray += [pscustomobject]@{ Command = "Get-PublicIP"      			; Shorthand = "whereami"; Description = "Shows your public IP" }
	$HelpArray += [pscustomobject]@{ Command = "MSBuild"         			; Description = "Shortcut to msbuild" }
	$HelpArray += [pscustomobject]@{ Command = "Notepad"         			; Shorthand = "edit"; Description = "Opens preferred text editor" }
	$HelpArray += [pscustomobject]@{ Command = "Open-WindowsExplorer"		; Shorthand = "we"; Description = "Open Windows Explorer in the current directory" }
	$HelpArray += [pscustomobject]@{ Command = "Reload"          			; Description = "Reload scripts" }
	$HelpArray += [pscustomobject]@{ Command = "Remove-ProfileLoader"		; Description = "Remove profile loader" }
	$HelpArray += [pscustomobject]@{ Command = "Run-Elevated"     			; Shorthand = "admin, sudo"; Description = "Launch terminal in Admin-mode" }
	$HelpArray += [pscustomobject]@{ Command = "Set-ProfileLoader"			; Description = "Install environment profile loader script" }
	$HelpArray += [pscustomobject]@{ Command = "Sign-File"       			; Description = "Sign file with private key" }
	$HelpArray += [pscustomobject]@{ Command = "Start-Docker"   			; Description = "Start Docker" }
	$HelpArray += [pscustomobject]@{ Command = "Stop-Terminal"   			; Shorthand = "quit"; Description = "Stop terminal process (close window)" }
	$HelpArray += [pscustomobject]@{ Command = "Test-All"        			; Description = "Run all tests from current directory" }
	$HelpArray += [pscustomobject]@{ Command = "Unblock"         			; Description = "Unblocks all file in current or specified folder with subfolders" }
	$HelpArray += [pscustomobject]@{ Command = "Get-ToolsHelp"     			; Shorthand = "helpme"; Description = "Show this list again" }
	return $HelpArray
}

function Create-Certificate {
	if (!$IsAdmin) {
		"You need administrative privileges to run this operation"
		return
	}
	$mypwd = ConvertTo-SecureString -String "Trementa!:1234#" -Force -AsPlainText
	$cert = New-SelfSignedCertificate -CertStoreLocation cert:\currentuser\my -Subject "CN=Trementa Development Signing Certificate" -KeyAlgorithm RSA -KeyLength 2048 `
		-Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -KeyExportPolicy Exportable -KeyUsage DigitalSignature `
		-Type CodeSigningCert -NotAfter (Get-Date).AddMonths(120)
	$certSourcePath = "cert:\currentuser\my\$($cert.Thumbprint)"
	$certDestPath = "$PSScriptRoot\$($(Get-Item $PSCommandPath).Basename).pfx"
	Get-ChildItem -Path $certSourcePath | Export-PfxCertificate -FilePath $certDestPath -Password $mypwd -Force	>$null
	return $cert
}

function Sign-File {
	param
	(
		[Parameter(Mandatory = $true)]$filePath
	)

	if (!$IsAdmin) {
		"You need administrative privileges to run this operation"
		return
	}
	$pfxFile = "$PSScriptRoot\$($(Get-Item $PSCommandPath).Basename).pfx"
	$mypwd = ConvertTo-SecureString -String "Trementa!:1234#" -Force -AsPlainText	
	import-pfxcertificate -FilePath $pfxFile -Password $mypwd -CertStoreLocation cert:\CurrentUser\My 					>$null
	import-pfxcertificate -FilePath $pfxFile -Password $mypwd -CertStoreLocation cert:\LocalMachine\TrustedPublisher	>$null
	$cert = import-pfxcertificate -FilePath $pfxFile -Password $mypwd -CertStoreLocation cert:\LocalMachine\Root

	Set-AuthenticodeSignature $filePath @(Get-ChildItem "cert:\CurrentUser\My\$($cert.Thumbprint)" -codesign)[0] 		>$null
	"Signed $filePath"
}

function Copy-Modules {
	$moduleDestination = split-path $profile.CurrentUserAllHosts
	$moduleDestination = join-path $moduleDestination "Modules"
	"Installing modules in '$($moduleDestination)'"
	$root = split-path $PSCommandPath

	# Copy files
	$search = join-path $root *.ps1
	Get-ChildItem $search -file |
	ForEach-Object {
		$scriptDir = $_.BaseName
		$destination = join-path $moduleDestination "$scriptDir"
		"Installing module '$($scriptDir)'"
		New-Item $destination -itemtype "directory" -Force >$null
		$destFile = join-path $destination "$($scriptDir).psm1"
		Copy-Item $_ $destFile -Force >$null
		
		if ($IsAdmin) {
			Sign-File $destFile
		}

		$destManifest = join-path $destination "$($scriptDir).psd1"		
		"Creating module manifest '$($destManifest)'"
		New-ModuleManifest -RootModule "$($scriptDir).psm1" -Path "$destManifest" -ModuleVersion $moduleVersion -Author "Jörgen Pramberg" -CompanyName "Trementa AB" -Description "Development Toolkit" -FunctionsToExport "*"	
	}

	# Copy directories
	get-childitem $search -Directory | foreach-object {
		copy-item $_ $moduleDestination -Force >$null
		if($IsAdmin) {
			$destination = join-path $moduleDestination $_.BaseName
			# .psm1 or binaries; .exe or .dll
			get-childitem $destination -Exclude *.psd1 -File | foreach-object { Sign_File $_.FullName }
		}
		Import-Module $_.BaseName
	}
	
	"Modules installation successful"
}

function Remove-ProfileLoader {
	if (Test-Path $profile.CurrentUserAllHosts) {
		Set-Content $profile.CurrentUserAllHosts -Value ''
	}
}

function Set-ProfileLoader {
	param
	(
		[Parameter(Mandatory = $true)]$script
	)

	New-Item -itemtype Directory -Force -Path (Split-Path $profile.CurrentUserAllHosts) >$null
	$script | Set-Content $profile.CurrentUserAllHosts
	
	if ($IsAdmin) {
		Sign-File $profile.CurrentUserAllHosts
	}	
}

function License {
	'
MIT License

Copyright (c) 2022 Trementa AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
'
	Write-Host -NoNewLine "[A]ccept license agreement [R]eject license agreement"
	
	while ($true) {
		$key = [System.Console]::ReadKey($true).Key
		if ($key -eq 'R' -or $key -eq 'Enter') {
			Exit
		}
		if ($key -eq 'A') {
			""
			return
		}
		$key
	}
}

function Export {
	Export-ModuleMember "Clean-Solution"
	Export-ModuleMember "Copy-Modules"
	Export-ModuleMember "Create-Certificate"
	Export-ModuleMember "Create-Configuration"
	Export-ModuleMember "Edit-Profile"
	Export-ModuleMember "Edit-Solution"
	Export-ModuleMember "Get-CommandDefinition"
	Export-ModuleMember "Get-GitRepositories"
	Export-ModuleMember "Get-GitStatus"
	Export-ModuleMember "Get-Help"
	Export-ModuleMember "Get-TerminalPath"
	Export-ModuleMember "Get-Prompt"
	Export-ModuleMember "Get-WindowTitle"
	Export-ModuleMember "Get-PublicIP"
	Export-ModuleMember "Git-Diff"
	Export-ModuleMember "Install-Tools"
	Export-ModuleMember "Invoke-Initialize"
	Export-ModuleMember "Is-Admin"
	Export-ModuleMember "License"
	Export-ModuleMember "New-Terminal"
	Export-ModuleMember "Open-WindowsExplorer"
	Export-ModuleMember "Refresh-Env"
	Export-ModuleMember "Reload"
	Export-ModuleMember "Remove-ACL"
	Export-ModuleMember "Remove-ProfileLoader"
	Export-ModuleMember "Run-Elevated"
	Export-ModuleMember "Select-Solution"
	Export-ModuleMember "Set-DefaultEditor"
	Export-ModuleMember "Set-Devenv"
	Export-ModuleMember "Set-ProfileLoader"
	Export-ModuleMember "Sign-File"
	Export-ModuleMember "Start-Docker"
	Export-ModuleMember "Stop-Terminal"
	Export-ModuleMember "Test-All"
	Export-ModuleMember "prompt"
	Export-ModuleMember "unblock"
}

function Popit
{
	pop-location -StackName devstack
}

function Alias{
	Set-Alias -Name admin -Value Run-Elevated -Scope Global
	Set-Alias -Name cln -Value Clean-Solution -Scope Global
	Set-Alias -Name edit -Value notepad -Scope Global
	Set-Alias -Name gs -Value Get-GitStatus -Scope Global
	Set-Alias -Name gd -Value Git-Diff -Scope Global
	Set-Alias -Name helpme -Value Get-ToolsHelp -Scope Global
	Set-Alias -Name pop -Value Popit -Scope Global
	Set-Alias -Name quit -Value Stop-Terminal -Scope Global
	Set-Alias -Name sln -Value Edit-Solution -Scope Global
	Set-Alias -Name sudo -Value admin -Scope Global
	Set-Alias -Name we -Value Open-WindowsExplorer -Scope Global
	Set-Alias -Name whereami -Value Get-PublicIP -Scope Global
	Set-Alias -Name which -Value Get-CommandDefinition -Scope Global
}

function Get-ToolsHelp {
	"Trementa Development Tools version $($moduleVersion)"
	"Powershell $($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion)"
	Invoke-Initialize
	Get-Commands | Format-Table -Property Command, Shorthand, Description -AutoSize
}

function Install-Other
{
	if($isadmin) {
		#Install-Module -Name PoshRSJob
		}
	else { "Run script in elevated mode to install external modules" }
}

function Install-Tools {
	License

	"This will install $($(Get-Item $PSCommandPath).Basename) modules version $($moduleVersion)"
	if (!$IsAdmin) {
		Write-Host -ForegroundColor Red -BackgroundColor Black "*** You are not running in administrative mode and therefore the modules won't be signed"
	}
	Write-Host -NoNewLine "Are you sure? [Y]es [N]o"

	while ($true) {
		$key = if ($continue) {
			Write-Host -NoNewLine "Y"
			'Y'
		}
		else {
			[System.Console]::ReadKey($true).Key
		}

		if ($key -eq 'N' -or $key -eq 'Enter') {
			return
		}
		if ($key -eq 'Y') {
			if ($IsAdmin) {	Create-Certificate >$null }
			Copy-Modules
			Install-Other
			$script = 
				"`$root = ""$defaultRoot""`n$((Get-Item $PSCommandPath ).Basename)\Invoke-Initialize"
			Set-ProfileLoader $script
			if($exit) {	exit }
			"Reloading profile..."
			Reload
		}
		# Debug mode
		if ($key -eq 'S') {
			"Overwriting default profile"
			$script = '"Running all ps1 scripts from `"' + $defaultRoot + '`""
						$root = "' + $defaultRoot + '"
						Get-ChildItem "$defaultRoot\*.ps1" | %{.$_}' 						
			Set-ProfileLoader $script
			"Reloading profile..."
			Reload
		}
	}
}

function Invoke-Initialize {
	Set-DefaultEditor
	Set-Devenv
	Alias
	Enable-dotnet-TabCompletion
}

function Is-Admin
{
	$isadmin = if ($PSVersionTable.PSEdition -eq 'Desktop' -or $PSVersionTable.Platform -eq 'Win32NT') {
		$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
		$principal = New-Object Security.Principal.WindowsPrincipal $identity
		$principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
	}
	elseif ($PSVersionTable.Platform -eq 'Unix') {
		(id -u) -eq 0
	}
	else {
		Write-Error "Cannot determine user mode for platform $($PSVersionTable.Platform)"
		$false
	}
	
	return $isadmin
}

function Refresh-Env{ $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH','machine') }

function Initialize
{
	$script:moduleVersion = "3.1.2"
	$script:isModule = $MyInvocation.MyCommand.Name.EndsWith('.psm1')
	$script:CACHE_VARIABLE_NAME = "TrementaDevelopment_Cache"

	if (![Environment]::Is64BitProcess) {
		throw @"
32-bit process detected
Version $($moduleVersion) module will only run in a 64-bit process
"@
	}

	$script:isadmin = Is-Admin
	# Called from command-line? Then install.
	if ($script:myinvocation.CommandOrigin -eq 0 -Or $install) {
		Install-Tools
		return
	}

	if ($HOME -eq (Get-Location).Path) {
		Set-Location $root
	}
	
	if ($isModule) { Export }
	Invoke-Initialize
}

Initialize
