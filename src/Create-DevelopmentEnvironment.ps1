#Requires -RunAsAdministrator

<#
.Synopsis
	Setup new system
.Description
	Installs all applications and dependencies specified
	
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

Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser

function InstallChocolatey {
	try {
		choco config get cacheLocation >$null
	}
	catch {
		iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
	}
}

function InstallScoop {
	try {
		scoop >$null
	}
	catch {
		irm get.scoop.sh -outfile 'install.ps1'
		iex "& {$(irm get.scoop.sh)} -RunAsAdmin"
	}
}

function RestartIfPendingReboot {
		function TestIfPendingReboot {
			if (get-childitem "hklm:\software\microsoft\windows\currentversion\component based servicing\rebootpending" -ea ignore) { return $true }
			if (get-item "hklm:\software\microsoft\windows\currentversion\windowsupdate\auto update\rebootrequired" -ea ignore) { return $true }
			if (get-itemproperty "hklm:\system\currentcontrolset\control\session manager" -name pendingfilerenameoperations -ea ignore) { return $true }
			try { 
				$util = [wmiclass]"\\.\root\ccm\clientsdk:ccm_clientutilities"
				$status = $util.determineifrebootpending()
				if (($status -ne $null) -and $status.rebootpending) {
					return $true
				}
			}
			catch {
				return $false
			}
		}

	if(TestIfPendingReboot) {
		"Pending reboot detected."
		"Press [Y]es if you want to restart the computer and then run this script again to continue."
		$key = [System.Console]::ReadKey($true).Key
		
		if ($key -eq 'Y') {		
			Restart-Computer
		}
		else {
			"Skipping reboot, notice that some features might not work until after rebooting."
		}
	}
}

function InstallWithCheck {
	if(($chocoPackages -like ($args[0] + '*')).Count -eq 0) {
		Invoke-Expression ("choco install " + ($args -Join ' '))
	}
	else {
		"Skipped " + $args[0] + ", already installed"
	}
}

InstallChocolatey
$chocoPackages = choco list -li
InstallWithCheck ditto -y --package-parameters '/VERYSILENT /NORESTART /MERGETASKS=addfirewallexception'
InstallWithCheck notepadplusplus -y
InstallWithCheck git -y
InstallWithCheck nodejs -y
InstallWithCheck pwsh -y
InstallWithCheck visualstudio2022community-preview -y --pre --package-parameters '--allWorkloads --includeRecommended --includeOptional --passive --locale en-US'
InstallWithCheck vscode -y
InstallWithCheck sql-server-management-studio -y
InstallWithCheck sqlitebrowser -y
InstallWithCheck postman -y
InstallWithCheck paint.net -y
InstallWithCheck spotify -y
InstallWithCheck telegram -y
InstallWithCheck messenger -y

$chocoPackages = choco list -li --source WindowsFeatures
InstallWithCheck HypervisorPlatform -y --source WindowsFeatures
InstallWithCheck VirtualMachinePlatform -y --source WindowsFeatures
InstallWithCheck Microsoft-Hyper-V-All -y --source WindowsFeatures
InstallWithCheck Containers -y --source WindowsFeatures
InstallWithCheck Containers-DisposableClientVM -y --source WindowsFeatures
InstallWithCheck Microsoft-Windows-Subsystem-Linux -y --source WindowsFeatures
InstallWithCheck wsl2 -y
InstallWithCheck wsl-ubuntu-2004 -y
InstallWithCheck docker-desktop -y

RestartIfPendingReboot

InstallScoop
scoop install winget
scoop bucket add extras
scoop install windows-terminal
winget upgrade --all

reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced /v HideFileExt /t REG_DWORD /d 0 /f
show-windowsdeveloperlicenseregistration
reg add HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock /t REG_DWORD /f /v AllowDevelopmentWithoutDevLicense /d 1	

register-argumentcompleter -native -commandname dotnet -scriptblock {
	 param($commandname, $wordtocomplete, $cursorposition)
		 dotnet complete --position $cursorposition "$wordtocomplete" | foreach-object {
			[system.management.automation.completionresult]::new($_, $_, 'parametervalue', $_)
	 }
}
