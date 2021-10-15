<#
.Synopsis
	Custom general profile with added functionality for development.
.Description
	Multiple helper tools for different development tasks.
	
.Todo
	Isolate project specific features in a separate script.
	
.License
	MIT License

	Copyright (c) 2020 Trementa AB

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
	[string] $defaultRoot = $(if ($_ -eq $null -Or -Not (Test-Path $_ -PathType Container)) { Get-Location } else { $_ }),
	[Switch] $createEnvJson,
	[Switch] $install,
	[Switch] $continue
)

<#
.SYNOPSIS
	Launch powershell elevated (administrator mode)
#>
function Run-Elevated {
	Start-Process (Get-PowershellName) -WorkingDirectory (Get-Location).Path -Verb runAs -ArgumentList "-NoExit", "-Command", ("cd " + (Get-Location).Path)
}

<#
.SYNOPSIS
	Launch new powershell instance
#>
function New-Powershell {
	Start-Process (Get-PowershellName) -WorkingDirectory (Get-Location).Path -ArgumentList "-NoExit", "-Command", ("cd " + (Get-Location).Path)
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
	# function Get-PowershellName {
		# (get-process -Id $Pid).ProcessName
		
		# function f {
			# $ParentProcessIds = Get-CimInstance -Class Win32_Process -Filter "Name = 'powershell.exe'"
		# }	
	# }

	Get-Process -Id $PID | Select-Object -ExpandProperty Path | ForEach-Object { Invoke-Command { & "$_" } -NoNewScope }
	
	# & (Get-PowershellName)	
}

<#
.SYNOPSIS
	Default function name for custom Powershell prompt
#>
function prompt {	
	<#
	.SYNOPSIS
		Create configuration hierarchially from all env.json files.
	#>
	function Create-Configuration {
		function Search($Path)
		{
			if ($Path) {
				Search(Split-Path $Path -Parent)
				Set-FolderLabels $Path
			}
		}
		
		function Set-FolderLabels {
			param([string] $Path)
			$envJson = Join-Path $Path 'env.json'
			if (Test-Path $envJson) {
				Push-Location $Path
				Get-Content $envJson | ConvertFrom-Json | Get-ObjectMembers | ForEach-Object {
					$Path = Resolve-Path $_.Value
					if ($Path -and (Test-Path $Path)) {
						Invoke-Expression "function global:$($_.Key) {Set-Location '$($Path)'}"
						if ($isModule) { Export-ModuleMember $_.Key }
						Set-Alias -Name $_.Key -Value "global:$($_.Key)" -Scope Global
					}
				}
				Pop-Location
			}
		}
		
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
		Search(Get-Location)			
	}

	<#
	.SYNOPSIS
		Set powershell window title.
	#>
	function Set-WindowTitle {
		function Get-WindowTitle {
			if ($isAdmin) {
				"[ADMIN] "
			}
			else {
				""
			}
			
			$isGitRepository = git rev-parse --is-inside-work-tree
			if ($LastExitCode -eq 128 -or $isGitRepository -eq $false) {
				"Not a Git repository"
			}
			else {
				$symbolicref = git symbolic-ref HEAD
				if ($NULL -ne $symbolicref) {
					"'" + (Split-Path -Leaf (git remote get-url origin)) + "' "
					$symbolicref = $symbolicref.substring($symbolicref.LastIndexOf("/") + 1)
					$symbolicref + ": "
					$status = git status --porcelain #--untracked-files=all

					if ( $status ) {
						$matches = [regex]::matches([system.string]::join("`n", $status), "(?m)^.{2}")
						$statusTotals = @{} # Create hash table

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
							[string]::format("{0}:{1} ", $dictEntry.Name, $dictEntry.Value)
						}
					}
					else {
						"nothing to commit (working dir clean)"
					}
				}
			}
		}
		
		$Host.UI.RawUI.WindowTitle = Get-WindowTitle
	}

	<#
	.SYNOPSIS
		Clear solution cache after a repository change.
	#>
	function ManageRepositoryChanges {
		function Get-GitRepoInfo {
			git remote show origin | 
			foreach-object {
				$repoName   = if ($_ -match "fetch url: .*\/(.*)") { $matches[1] }
				$fetchUrl   = if ($_ -match "fetch url: (.*)") { $matches[1] }
				$pushUrl    = if ($_ -match "push  url: (.*)") { $matches[1] }
				$headBranch = if ($_ -match "head branch: (.*)") { $matches[1] }
			}

			$branchName = git symbolic-ref HEAD
			$branchName = $branchName.substring($branchName.LastIndexOf("/") +1)
			
			@{ 
				branchName = $branchName
				fetchUrl   = $fetchUrl
				headbranch = $headbranch
				pushUrl    = $pushUrl
				repoName   = $repoName
			}	
		}
		
		$gitSource = Get-GitRepoInfo
		if ($null -ne $gitSource) {
			# Did the branch change?
			if ($gitSource -ne $script:gitSource) {
				Write-Host "Clearing solution cache: git source changed from $($script:gitSource) to $($gitSource)"
				Set-Variable -Name $script:CACHE_VARIABLE_NAME -Scope Global -Value @{}
				$script:gitSource = $gitSource
			}
		}
	}

	Create-Configuration >$null
	Set-WindowTitle >$null

	# History ID
	$HistoryId = $MyInvocation.HistoryId
	# Uncomment below for leading zeros
	# $HistoryId = '{0:d4}' -f $MyInvocation.HistoryId
	Write-Host -Object "$HistoryId`: " -NoNewline -ForegroundColor Cyan
 
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
		
		$devEnvPath = & $vswhere -latest -prerelease -property installationPath
		if (Test-Path "$devEnvPath\MSBuild") {
			# Quick search
			$msbuilds = Get-Childitem -Path "$devEnvPath\MSBuild" -Include MSBuild.exe -File -Recurse -ErrorAction SilentlyContinue | Where-Object DirectoryName -NotLike *amd64*
		}
		else {
			$msbuilds = Get-Childitem -Path $devEnvPath -Include MSBuild.exe -File -Recurse -ErrorAction SilentlyContinue | Where-Object DirectoryName -NotLike *amd64*
		}
		 
		if ($msbuilds -is [array]) {
			$msbuild = $msbuilds[0]
		}
		else {
			$msbuild = $msbuilds
		}
		Set-Alias -Name MSBuild -Value $msbuild -Scope Global
	}
	else {
		Write-Host "Visual Studio not installed" -ForegroundColor Red
	}
}

<#
.SYNOPSIS
	Show all solutions in folder.
	Run as background job?
#>
function Select-Solution {
	# $solutions = GetOrAdd-Cache -Key (Get-Location).Path -ScriptBlock {
	$solutionsGenerator = {
		$global:counter = 1;
		$cd = (Get-Location).Path.Length
		return @(Get-ChildItem *.sln -Recurse | Sort-Object -Property Name |
			Select-Object @{Name = "Id"; Expression = { $global:counter; $global:counter++ } }, Name, FullName, @{Name = "Path"; Expression = { $_.FullName.SubString($cd) } })
	}

	$solutions = $solutionsGenerator.Invoke()
	if ($solutions.Count -eq 0) {
		return ""
	}
	if ($solutions.Count -gt 1) {
		$solutions | Format-Table -Property @{Label = "#"; Expression = { $_.Id } }, Name, Path -AutoSize | Out-Host
		do {
			$input = Read-Host "Select solution (1-$($solutions.Count))"
			if (!$input) {
				return $null
			}
			$value = $input -as [Int]		
			$valid = $value -ne $NULL -and $value -le $solutions.Count -and $value -gt 0
		} until($valid)
	}
	else {
		$value = 1
	}
	return $($solutions[$value - 1].FullName)

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
}

<#
.SYNOPSIS
	Select solution and open with Visual Studio.
#>
function Edit-Solution {
	param
	(
		[string] $solution
	)

	if (!$solution) { $solution = Select-Solution }
	if ($solution) {
		Write-Host "Opening solution $solution"
		DevEnv "$solution"
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

function Get-Commands {
	[PsObject[]]$HelpArray = @()
	$HelpArray += [pscustomobject]@{ Command = "Clean-Solution"  			; Shorthand = "cln"; Description = "Clean solution structure from artifacts" }
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
	$HelpArray += [pscustomobject]@{ Command = "Run-Elevated"     			; Shorthand = "admin, sudo"; Description = "Launch PowerShell in Admin-mode" }
	$HelpArray += [pscustomobject]@{ Command = "Set-ProfileLoader"			; Description = "Install environment profile loader script" }
	$HelpArray += [pscustomobject]@{ Command = "Sign-File"       			; Description = "Sign file with private key" }
	$HelpArray += [pscustomobject]@{ Command = "Start-Docker"   			; Description = "Start Docker" }
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
	$search = join-path $root *.ps1
	Get-ChildItem $search |
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
		New-ModuleManifest -RootModule "$($scriptDir).psm1" -Path "$destManifest" -ModuleVersion $moduleVersion -Author "Jörgen Pramberg" -CompanyName "Trementa AB" -Description "Trementa Development Toolkit" -FunctionsToExport "*"	
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

Copyright (c) 2020 Trementa AB

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
	Export-ModuleMember "Edit-Profile"
	Export-ModuleMember "Edit-Solution"
	Export-ModuleMember "Get-CommandDefinition"
	Export-ModuleMember "Get-GitRepositories"
	Export-ModuleMember "Get-Help"
	Export-ModuleMember "Get-PowershellName"
	Export-ModuleMember "Get-PublicIP"
	Export-ModuleMember "Git-Diff"
	Export-ModuleMember "Install-Tools"
	Export-ModuleMember "Invoke-Initialize"
	Export-ModuleMember "License"
	Export-ModuleMember "New-Powershell"
	Export-ModuleMember "Open-WindowsExplorer"
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
	Export-ModuleMember "Test-All"
	Export-ModuleMember "prompt"
	Export-ModuleMember "unblock"
}

function Alias{
	Set-Alias -Name admin -Value Run-Elevated -Scope Global
	Set-Alias -Name cln -Value Clean-Solution -Scope Global
	Set-Alias -Name edit -Value notepad -Scope Global
	Set-Alias -Name gd -Value Git-Diff -Scope Global
	Set-Alias -Name helpme -Value Get-ToolsHelp -Scope Global
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

function Install-ExternalModules
{
	if($isadmin) { Install-Module -Name PoshRSJob }
	else { "Run script in elevated mode to install external modules" }
}

function Install-Tools {
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
			License
			if ($IsAdmin) {	Create-Certificate >$null }
			Copy-Modules
			# Install-ExternalModules
			$script = 
				"`$root = ""$defaultRoot""`n$((Get-Item $PSCommandPath ).Basename)\Invoke-Initialize"
			Set-ProfileLoader $script
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
}

function Initialize
{
	$script:moduleVersion = "3.0.1"
	$script:isModule = $MyInvocation.MyCommand.Name.EndsWith('.psm1')
	$script:CACHE_VARIABLE_NAME = "TrementaDevelopment_Cache"

	if (![Environment]::Is64BitProcess) {
		throw @"
32-bit process detected
Version $($moduleVersion) module will only run in a 64-bit process
"@
	}

	$script:isadmin = if ($PSVersionTable.PSEdition -eq 'Desktop' -or $PSVersionTable.Platform -eq 'Win32NT') {
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
