$ChocolateyPackageId = "ditto"

$chocolateyBin = [Environment]::GetEnvironmentVariable("ChocolateyInstall", "Machine") + "\bin"
if(-not (Test-Path $chocolateyBin)) {
    Write-Output "Environment variable 'ChocolateyInstall' was not found in the system variables. Attempting to find it in the user variables..."
    $chocolateyBin = [Environment]::GetEnvironmentVariable("ChocolateyInstall", "User") + "\bin"
}

$cinst = "$chocolateyBin\cinst.exe"
$choco = "$chocolateyBin\choco.exe"

if (-not (Test-Path $cinst) -or -not (Test-Path $choco)) {
    throw "Chocolatey was not found at $chocolateyBin."
}

if (-not $ChocolateyPackageId) {
    throw "Please specify the ID of an application package to install."
}

$chocoVersion = & $choco --version
Write-Output "Running Chocolatey version $chocoVersion"

$chocoArgs = @()
if([System.Version]::Parse($chocoVersion) -ge [System.Version]::Parse("0.9.8.33")) {
    Write-Output "Adding --confirm and --exit-when-reboot-detected arguments passed to Chocolatey"
    $chocoArgs += @("-y", "--exit-when-reboot-detected", "")
}

if (-not $ChocolateyPackageVersion) {
    Write-Output "Installing package $ChocolateyPackageId from the Chocolatey package repository..."
    & $cinst $ChocolateyPackageId $($chocoArgs)
} else {
    Write-Output "Installing package $ChocolateyPackageId version $ChocolateyPackageVersion from the Chocolatey package repository..."
    & $cinst $ChocolateyPackageId -Version $ChocolateyPackageVersion $($chocoArgs) --force
}

$codes = $IgnoreExitCodes -split ','

if ($codes -Contains $lastExitCode)
{
    exit 0
}