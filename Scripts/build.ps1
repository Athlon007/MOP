$MSBuildPath = 'C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe'
$SLNFile = '..\MOP.sln'
$BuildFolder = '..\build'

function Build-Mop{
    param(
        [Parameter (Mandatory = $true)] [String]$Configuration,
        [Parameter (Mandatory = $true)] [String]$ModLoader,
        [Parameter (Mandatory = $true)] [String]$OutputArchive
    )

    $PathToTheDll = "..\MOP\bin\$Configuration\MOP.dll";

    if (Test-Path $PathToTheDll) {
        Remove-Item $PathToTheDll
    }

    if (Test-Path "$BuildFolder\$OutputArchive") {
        Remove-Item "$BuildFolder\$OutputArchive"
    }

    Write-Host "BUILDING MOP FOR $ModLoader...`n`n" -ForegroundColor red
    Invoke-Expression -Command "& '$MSBuildPath' '$SLNFile' /property:Configuration=$Configuration"

    if (Test-Path "$PathToTheDll") {
        Write-Host "PACKING..."
        Compress-Archive -Path "$PathToTheDll" -DestinationPath "$BuildFolder\$OutputArchive" -Force
        Write-Host "DONE!`n`n"
    } else {
        Write-Host "=== COULD NOT BUILD MOP FOR $ModLoader! ===" -ForegroundColor red
    }

    Get-FileHash "$BuildFolder\$OutputArchive" -Algorithm SHA256 | Select-Object Hash | Out-File -FilePath "$BuildFolder\$OutputArchive.sha256.txt"
}

Build-Mop 'Release' 'MSCLoader' 'MOP.zip'
Build-Mop 'ProRelease' 'Mod Loader Pro' 'MOP.pro.zip'

Invoke-Item $BuildFolder