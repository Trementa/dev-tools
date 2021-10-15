<#
.Synopsis
   Play sound on Windows Media Player in backgroud.
.DESCRIPTION
   Play-Sound Cmdlet uses Windows Media Player to play background sound.
.PARAMETER SoundFile
    File to play
#>
function Play-Sound {
    [cmdletbinding()]
	Param(
	[String] $SoundFile,
	[int] $MillisecondsToWait = 10000)
	
	$init = {                  
		function PlayMusic($SoundFile) {
			Add-Type -AssemblyName PresentationCore
			$MediaPlayer = New-Object System.Windows.Media.Mediaplayer
					$MediaPlayer.Open($SoundFile)
					$MediaPlayer.Play()
					Start-Sleep -Seconds $MillisecondsToWait
					$MediaPlayer.Stop()
		}
	}

	Start-Job -Name PlaySound -InitializationScript $init -ScriptBlock {playmusic $args[0] $args[1]} -ArgumentList $SoundFile, $MillisecondsToWait
	Start-Sleep -Seconds 3
}

# Show notification balloon
function Show-NotifyBalloon($Message) {
	[system.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms') | Out-Null            
	$Global:Balloon = New-Object System.Windows.Forms.NotifyIcon            
	$Balloon.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon((Get-Process -id $pid | Select-Object -ExpandProperty Path))                    
	$Balloon.BalloonTipIcon = 'Info'           
	$Balloon.BalloonTipText = $Message            
	$Balloon.BalloonTipTitle = 'Now Playing'            
	$Balloon.Visible = $true            
	$Balloon.ShowBalloonTip(1000)
}

Export-ModuleMember -Function "Play-Sound"
Export-ModuleMember -Function "Show-NotifyBalloon"
