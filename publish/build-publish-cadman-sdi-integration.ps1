# Version numbers parts can have a max of 65534, the total seconds of a day could add up until 86400
# therefor we divide the elapsed seconds by 2
$elapsedSecondsHalf = [math]::Truncate( (Get-Date).TimeOfDay.TotalSeconds / 2)
$buildVersion = (Get-Date -Format "yyyy.MM.dd") + "." + $elapsedSecondsHalf
$buildVersion
# Create directory if not exists
New-Item -ItemType Directory -Force -Path ".\build-published"
# Remove files/folders inside the published folder
Remove-Item ".\build-published\*" -Recurse -Force
dotnet publish ../src/CadmanSDI.Host -r win-x64 -p:PublishSingleFile=true /p:Version=$buildVersion -f net10.0 -p:IncludeAllContentForSelfExtract=true -c Release /p:DebugType=none /p:DebugSymbols=false -o ./build-published/cadman-sdi-integration
Copy-Item "../src/CadmanSDI.Host/appsettings.json" -Destination "./build-published/cadman-sdi-integration"

New-Item -ItemType Directory -Force -Path "./build-published/cadman-sdi-integration/data"
Copy-Item "../src/CadmanSdi.Host/Data/*" -Destination "./build-published/cadman-sdi-integration/data" -Recurse
# Create zip file
Compress-Archive -DestinationPath "./build-published/cadman-sdi-integration.zip" -Path "./build-published/cadman-sdi-integration/*" -CompressionLevel Optimal -Force
# Write the version to text file, this can be added to the source control
Set-Content -Path "./published-latest-version.txt" -Value $buildVersion