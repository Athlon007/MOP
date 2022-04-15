    Write-Host "Packing MOP..."
    Compress-Archive -Path "..\MOP\bin\Release\MOP.dll" -DestinationPath "..\build\MOP.zip" -Force
    Compress-Archive -Path "..\MOP\bin\ProRelease\MOP.dll" -DestinationPath "..\build\MOP.pro.zip" -Force
    Write-Host "Done!"