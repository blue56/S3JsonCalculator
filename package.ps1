$date = Get-Date
$version = $date.ToString("yyyy-dd-M--HH-mm-ss")
$filename = "S3JsonCalculator-" + $version + ".zip"
cd .\S3JsonCalculator\src\S3JsonCalculator
dotnet lambda package ..\..\..\Packages\$filename --configuration Release -frun dotnet6 -farch arm64
cd ..\..\..