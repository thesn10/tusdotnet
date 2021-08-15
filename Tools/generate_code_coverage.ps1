# Framework to use when generating coverage. Must be a "real" framework, i.e. not netstandard.
$framework = "netcoreapp3.1";
# The file path passed to coverlet...
$coverletWantedOutput = "$PSScriptRoot\coverage.xml";
# ...which will result in the following filename.
$coverletActualOutput = "$PSScriptRoot\" + "coverage.{framework}.xml".Replace("{framework}", $framework);
Write-Host $coverletWantedOutput;
Write-Host $coverletActualOutput;
If (Test-Path $coverletActualOutput) {
	Remove-Item $output -Force
}

dotnet test -f $framework /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=$coverletWantedOutput /p:Exclude="[xunit.*]*" ../source/tusdotnet.sln

$key = Read-Host -Prompt 'Input Codecov key'

if(-not (Test-Path $coverletActualOutput)) {
	throw "$coverletActualOutput was not found";
}
./codecov.sh -f $coverletActualOutput -t $key