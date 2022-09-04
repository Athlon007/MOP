$MSBuildPath = 'C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe'
$SLNFile = '..\MOP.sln'
$BuildFolder = '..\build'

function Build-Mop{
    $PathToTheDll = '..\MOP\bin\$Configuration\MOP.dll';

    if (Test-Path $PathToTheDll) {
        Remove-Item $PathToTheDll
    }

    param(
        [Parameter (Mandatory = $true)] [String]$Configuration,
        [Parameter (Mandatory = $true)] [String]$ModLoader,
        [Parameter (Mandatory = $true)] [String]$OutputArchive
    )

    Write-Host "BUILDING MOP FOR $ModLoader...`n`n" -ForegroundColor red
    Invoke-Expression -Command "& '$MSBuildPath' '$SLNFile' /property:Configuration=$Configuration"

    Write-Host "PACKING..."
    Compress-Archive -Path "$PathToTheDll" -DestinationPath "$BuildFolder\$OutputArchive" -Force
    Write-Host "DONE!`n`n"
}

Build-Mop 'Release' 'MSCLoader' 'MOP.zip'
Build-Mop 'ProRelease' 'Mod Loader Pro' 'MOP.pro.zip'

Invoke-Item $BuildFolder