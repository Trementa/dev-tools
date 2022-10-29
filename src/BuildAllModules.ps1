$ErrorFile = New-TemporaryFile
$OutputFile = New-TemporaryFile
$successfuls = 0
$failures = 0

Get-ChildItem -Recurse *Module.csproj |
ForEach-Object {
	"Building module $($_.Basename)"
	
	dotnet build $_.FullName --configuration Release >$OutputFile 2>$ErrorFile
	if ($LastExitCode -ne 0)
	{
		Write-Error "$($_.Basename) build failed"
		$failures++
	}
	else
	{
		Write-Output "$($_.Basename) build succeeded"
		$successfuls++
	}
}

if($failures -gt 0)
{
	Write-Error "$($failures) modules failed"
	Write-Error "Logfile: $($ErrorFile.FullName)"
}

if($successfuls -gt 0)
{
	Write-Output "$($successfuls) modules succeeded"
}

Write-Host "Build log: $($OutputFile.FullName)"
