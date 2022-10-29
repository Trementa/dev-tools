function devenv {
	function Get-VisualStudioInstances {
		$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
		if (Test-Path $vswhere) {
			$vsJson = & $vswhere -prerelease -sort -utf8 -format json
			$vsInfo = convertfrom-json ('{"data":' + ($vsJson -join " ") + "}")
			return $vsInfo
		}
		else {
			throw "Can't find Visual Studio"
		}
	}

	if(!$vsInfo) { $vsInfo = Get-VisualStudioInstances }

	if($vsversion) {
		
	}

	if($selectedVSIndex -eq -1) {
		$script:c = 1;
		$data = $vsInfo.Data | Select-Object @{Name = "Id"; Expression = { $script:c; $script:c++ } }, displayName, installationVersion, productPath, "j"
		
		if ($data.Count -eq 0) {
			throw "Can't find any installed instances of Visual Studio"
		}
		if ($data.Count -gt 1) {
			$data | Format-Table -Property @{Label = "#"; Expression = { $_.Id } }, @{Label = "Visual Studio instance"; Expression = { $_.displayName } }, @{Label = "Version"; Expression = { $_.installationVersion } } | Out-Host
			do {
				$input = Read-Host "Select Visual Studio instance (1-$($data.Count))"
				if (!$input) {
					return $null
				}
				$value = $input -as [Int]		
				$valid = $value -ne $NULL -and $value -le $data.Count -and $value -gt 0
			} until($valid)
		}
		else {
			$value = 1
		}
		$selectedVSIndex = $value - 1
	}
	
	#$vsInfo.Data[$selectedVSIndex]

	return & $vsInfo.Data[$selectedVSIndex].productPath
}

function Apply-Configuration
{
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
	    $labelDirMapping = @{}
	    $envJson = Join-Path $cd 'devenv.config'
	    if (Test-Path $envJson) {
		    push-location $cd
		    try {
				$json = Get-Content $envJson | ConvertFrom-Json
				if($json.visualstudio) { $vsv = $json.visualstudio }
			    ($json.folders) | Get-ObjectMembers | ForEach-Object {
				    if($_.Value){
					    $absPath = Resolve-Path $_.Value
					    if (Test-Path $absPath) {
						    $labelDirMapping[$_.Key] = $absPath
					    }
				    }
			    }
		    } catch {}
		    pop-location
	    }
	    return ($labelDirMapping, $vsv)
    }

    function Search-Configuration {
	    param([string] $path)
	    if($path) {
		    (Search-Configuration(Split-Path $path -Parent)) + (Get-DevEnvConfig($path))
	    }
    }

	($labelMap, $vsversion) = Search-Configuration(Get-Location)
    foreach($label in $labelMap.Keys) {
		$path = $labelMap[$label]
		Invoke-Expression "function global:set_$($label) {Set-Location '$($path)'}"
		if ($isModule) { Export-ModuleMember $label }
		Set-Alias -Name $label -Value "set_$($label)" -Scope Global
	}
}

Create-LabelAliases

Get-VisualStudioInstances

#"Start job...."
#run-jobs
"......."
#"Launch...."
#devenv1

#msbuild

#$appJob.ChildJobs[0].Output
