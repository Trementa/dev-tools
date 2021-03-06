#Requires -RunAsAdministrator
param($config = "windows-packages.config")

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

function Invoke-Install
{
	Param($appFeaturesAndScripts)

	try{
		choco config get cacheLocation
	}catch{
		Write-Output "Chocolatey not detected, trying to install now"
		iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
	}

	foreach ($app in $appFeaturesAndScripts.features)
	{
		Write-Host "Installing $app"
		& choco install $app /y /source windowsfeatures | Write-Output
	}

	foreach ($app in $appFeaturesAndScripts.applications)
	{
		Write-Host "Installing $app"
		& choco install $app /y | Write-Output
	}
	
	foreach ($script in $appFeaturesAndScripts.scripts)
	{
		Write-Host "Running script: $script"
		Invoke-Expression $script
	}	
}

Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser
$json = Get-Content $config | ConvertFrom-Json 
Invoke-Install $json
