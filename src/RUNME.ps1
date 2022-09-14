<#
.Synopsis
	Creates and updates GK development environment.
#>
param()

function is-admin
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

function refresh-env
{
	"----------------------"
	"Refreshing environment"
	"----------------------"
	$pwsh = Get-Process -Id $PID | Select-Object -ExpandProperty Path -First 1
	$env:PATH = [System.Environment]::GetEnvironmentVariable('PATH','machine') 
	Invoke-Command { & "$($pwsh)" } -NoNewScope
}

function restart-script
{
	$pwsh = Get-Process -Id $PID | Select-Object -ExpandProperty Path -First 1
	$cd = (Get-Location).Path
	Start-Process $pwsh -WorkingDirectory $cd -Verb runAs -Wait -ArgumentList "-NoExit", "-noprofile", "-Command", "cd $($cd);. $($PSCommandPath) -exit"
	return $pwsh
}

"-------------------------------------"
"Setting up GK development environment"
"-------------------------------------"

Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser

if(!(is-admin))
{
	"Re-launching script in elevated mode"
	"------------------------------------"
	restart-script
	"-------------------------"
	"Elevated process finished"
	"-------------------------"
	refresh-env
	return
}

push-location .\

# Default is SSH clone.
$gkkgrootrepo = "git@ssh.dev.azure.com:v3/gk-devs/GKCloud/gkkg-root"
$cloneWithSSH = $true

# Get latest version of gkkg-root
$cd = split-path -leaf "$(get-location)"
$root = "gkkg-root"

# First, check if it already exists or if we have to do a clean install.
if($cd -eq $root)
{
	$isUpdating = $true
}
elseif(!(Test-Path "gkkg-root"))
{
	"-------------"
	"Clean install"	
	"---------------------------------"
	"Prerequisites: Basic tool support"
	"---------------------------------"

	$isInstalling = $true
	try{
		choco config get cacheLocation
	}catch{
		Write-Output "Chocolatey not detected, trying to install now"
		iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
	}
	choco install git /y
	
	while(!$rootrepoexists)
	{
		try
		{
			git clone $gkkgrootrepo
			$rootrepoexists = $true
		}
		catch
		{
			"Couldn't clone repository."
			"Please select one of the options:"
			"(C)reate a new SSH key. Use (h)ttps. Use (s)sh. (E)xit this script."

			$key = [System.Console]::ReadKey($true).Key
			switch($key)
			{
				'C'
				{
					ssh-keygen -b 4096
					"Copy the public key and set this key for your user in Azure Devops before continuing."
					$rootrepoexists = $false
					$cloneWithSSH = $true
				}
				'H'
				{
					$gkkgrootrepo = "https://gk-devs@dev.azure.com/gk-devs/GKCloud/_git/gkkg-root"
					$rootrepoexists = $false
					$cloneWithSSH = $false
				}
				'S'
				{
					$gkkgrootrepo = "git@ssh.dev.azure.com:v3/gk-devs/GKCloud/gkkg-root"
					$rootrepoexists = $false
					$cloneWithSSH = $true
				}
				'E'
				{
					Exit
				}
			}
		}
	}
	set-location gkkg-root
}

if($isUpdating)
{
	"------------------"
	"Updating gkkg-root"
	"------------------"
	git checkout main
	git fetch --all
		
	if(git diff --exit-code RUNME.ps1)
	{
		"This script has changed."
		git checkout origin/main -- RUNME.ps1
		restart-script
		return
	}

	git pull
}


"-------------------------------------------------------"
"Installing default windows developer tools and features"
"-------------------------------------------------------"

. .\scripts\Default-Install.ps1 ".\configs\developer-applications.config"

"-------------------------------"
"Creating project root structure"
"-------------------------------"

# Create a root folder where each repository in the project has its own subfolder: NOT submodule as this imposes more problems than it solves!
# Each subfolder is added to .gitignore.
# Skip gkkg-root

".gitignore" > .gitignore
".vs/" > .gitignore
"nuget/" > .gitignore
"REMOVEME.ps1" > .gitignore

$getgitRepos = "https://dev.azure.com/gk-devs/GKCloud/_apis/git/repositories?api-version=1.0"
$pat = "OjJyZ3gyZWxmdDVuNzY3cDR3dHN2eHJncGxsamEzcmxrd2ZuY3dxdHM2N21ycHJheXpoNnE="
$headers = @{Authorization="Basic $($pat)"; Accept="application/json"}
$projects = Invoke-RestMethod -Uri $getgitRepos -Method get -Headers $headers
foreach($project in $projects.value)
{
	if($project.name -ne "gkkg-root")
	{
		"$($project.name)/" >> .gitignore
		if(Test-Path "$($project.name)")
		{
			git -C "$($project.name)" pull
		}
		else
		{
			if($cloneWithSSH) {	git clone $project.sshUrl }
			else { git clone $project.webUrl }
		}
	}
}

"-----------------------------------"
"Installs powershell developer tools"
"-----------------------------------"

. .\scripts\Dev-Tools.ps1 -defaultRoot .\ -install -continue -exit
refresh-env

if($isInstalling)
{
	"***************************************************"
	"Project initialization, setup and installation done"
	"***************************************************"
}
else
{
	"***********"
	"Update done"
	"***********"
}

pop-location
