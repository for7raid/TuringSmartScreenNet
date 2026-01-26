# ATTENTION: Don't forget to allow the powershell script to run first.
# Here is a step by step of how to execute this script:
# - Open a Powershell command prompt. 
# - Allow shel script execution with command:
#   Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process
# - Execute the current powershell script with the command:
#   .\GetBatteryLevel_DOQAUS-CARE1v1.ps1
# - The results may take some time to come. A pop-up will show on your 
#   main screen. You must click 'Ok' to unblock the Powershell command prompt.

$StartTime = Get-Date
$BTDeviceFriendlyName = "Studio Wireless"
$Shell = New-Object -ComObject "WScript.Shell"
$BTHDevices = Get-PnpDevice -FriendlyName "*$($BTDeviceFriendlyName)*"

$batteryStatusProperty = "{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2"
$connectionStatusProperty = "{83DA6326-97A6-4088-9453-A1923F573B29} 15"

if ($BTHDevices) {
    $BatteryLevels = foreach ($Device in $BTHDevices) {
		
        $BatteryProperty = Get-PnpDeviceProperty -InstanceId $Device.InstanceId -KeyName $connectionStatusProperty |
            Where-Object { $_.Type -ne 'Empty' } |
            Select-Object -ExpandProperty Data

if ($BatteryProperty) {
	write-host $Device.Name
	write-host $Device.InstanceId
            $BatteryProperty
        }
    }

if ($BatteryLevels) {
		$ElapsedTime = (Get-Date) - $StartTime
        $ElapsedTimeMilliseconds = [math]::Round($ElapsedTime.TotalMilliseconds, 0)
        $ElapsedTimeStr = "$($ElapsedTimeMilliseconds) ms"
        if ($ElapsedTimeMilliseconds -gt 1000) {
            $ElapsedTimeSeconds = $ElapsedTime.TotalSeconds
            $ElapsedTimeStr = "$($ElapsedTimeSeconds) sec"
        }
        $Message = "Battery Level of $($BTDeviceFriendlyName): $BatteryLevels %`nElapsed Time: $($ElapsedTimeStr)"
        Write-Host $message
    }
    else {
        Write-Host "No battery level information found for $($BTDeviceFriendlyName) devices."
    }
}
else {
    Write-Host "Bluetooth device found."
}