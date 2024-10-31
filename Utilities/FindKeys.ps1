# to run this program, you have to have the windows utility PsExec.exe in this folder
# https://learn.microsoft.com/en-us/sysinternals/downloads/psexec

# Define variables
$folderPath = "C:\\ProgramData\\Microsoft\\Crypto\\RSA\\MachineKeys"  # Replace with the folder you want to scan
$searchString = "\\ProgramData\\IoT Gateway\\"


# Create the PowerShell command to search for the string in files
$localCommand = "
Write-Output 'Starting search in folder: $folderPath'

Get-ChildItem $folderPath | 
Foreach-Object {
    `$content = Get-Content `$_.FullName
    if (`$content -Match '$searchString') {
        Write-Output `$_.FullName
        #Remove-Item -Path `$_.FullName -Force # BE WARY OF UNCOMMENTING THIS LINE, IF SOMETHING GOES WRONG, IT COULD POTENTIALY DELETE IMPORTANT FILES. PLEASE BACKUP BEFORE YOU RUN.
    }
}
"

# Run PsExec with -s (SYSTEM) to execute the command as SYSTEM
& ".\PsExec.exe" -s powershell.exe -NoProfile -Command $localCommand