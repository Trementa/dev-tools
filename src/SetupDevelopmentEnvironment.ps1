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

try {
	choco config get cacheLocation
}
catch {
	Write-Output "Chocolatey not detected, trying to install"
	iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

choco install HypervisorPlatform -y --source WindowsFeatures
choco install VirtualMachinePlatform -y --source WindowsFeatures
choco install Microsoft-Hyper-V-All -y --source WindowsFeatures
choco install Containers -y --source WindowsFeatures
choco install Containers-DisposableClientVM -y --source WindowsFeatures
choco install Microsoft-Windows-Subsystem-Linux -y --source WindowsFeatures

choco install wsl2 -y
choco install wsl-ubuntu-2004 -y
choco install notepadplusplus -y
choco install nodejs -y
choco install git -y
choco install docker-desktop -y
choco install visualstudio2022community-preview --pre --package-parameters '--allWorkloads --includeRecommended --includeOptional --passive --locale en-US' -y
choco install vscode -y
choco install sql-server-management-studio -y
choco install ditto -y
choco install office2019proplus -y
choco install teams -y
choco install spotify -y
choco install paint.net -y
choco install powershell-core -y
choco install microsoft-windows-terminal -y
choco install wsl2 -y
choco install wsl2 -y
choco install wsl2 -y
choco install wsl2 -y
choco install wsl2 -y

reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced /v HideFileExt /t REG_DWORD /d 0 /f
show-windowsdeveloperlicenseregistration
reg add HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock /t REG_DWORD /f /v AllowDevelopmentWithoutDevLicense /d 1
