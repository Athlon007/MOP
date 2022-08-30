$MSBuildPath = 'C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe'
$SLNFile = '..\MOP.sln'

Write-Host "BUILDING MOP FOR MSCLOADER...`n`n" -ForegroundColor red
Invoke-Expression -Command "& '$MSBuildPath' '$SLNFile' /property:Configuration=Release"
Write-Host "`n`nBUILDING MOP FOR MOD LOADER PRO...`n`n" -ForegroundColor red
Invoke-Expression -Command "& '$MSBuildPath' '$SLNFile' /property:Configuration=ProRelease"

Write-Host "`n`nBuilding done! Packing MOP..."
Compress-Archive -Path "..\MOP\bin\Release\MOP.dll" -DestinationPath "..\build\MOP.zip" -Force
Compress-Archive -Path "..\MOP\bin\ProRelease\MOP.dll" -DestinationPath "..\build\MOP.pro.zip" -Force
Write-Host "Done!"

Invoke-Item "..\build"