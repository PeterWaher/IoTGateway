# Define the name of the publish profile to look for
$publishProfileName = "Release win-x86.pubxml"

# Recursively find all .csproj files in the solution directory
$projects = Get-ChildItem -Recurse -Filter *.csproj

# Loop through each project and check for the publish profile
foreach ($project in $projects) {
    # Define the path to the publish profile
    $publishProfilePath = Join-Path -Path $project.Directory.FullName -ChildPath "Properties\PublishProfiles\$publishProfileName"
    
    # Check if the publish profile exists
    if (Test-Path $publishProfilePath) {
        Write-Host "Publishing $($project.FullName) using profile $publishProfileName"
        # Run dotnet publish with the specific profile
        dotnet publish $project.FullName -c Release -p:PublishProfile=$publishProfilePath
    } else {
        Write-Host "Skipping $($project.FullName) - publish profile $publishProfileName not found."
    }
}
