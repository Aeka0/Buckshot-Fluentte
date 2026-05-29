param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Get-Text([int[]]$Codes) {
    return (-join ($Codes | ForEach-Object { [char]$_ }))
}

$projectPath = Join-Path $PSScriptRoot "BuckshotFluentte.csproj"
$assetsPath = Join-Path $PSScriptRoot "obj\project.assets.json"
$outputPath = Join-Path $PSScriptRoot "bin\x64\$Configuration\net9.0-windows10.0.22621.0\win-x64"

$msgProjectMissing = Get-Text @(0x672A, 0x627E, 0x5230, 0x9879, 0x76EE, 0x6587, 0x4EF6, 0xFF1A)
$msgFastBuild = Get-Text @(0x68C0, 0x6D4B, 0x5230, 0x53EF, 0x590D, 0x7528, 0x7684, 0x8FD8, 0x539F, 0x7ED3, 0x679C, 0xFF0C, 0x4F7F, 0x7528, 0x5FEB, 0x901F, 0x7F16, 0x8BD1, 0x6A21, 0x5F0F, 0xFF08, 0x8DF3, 0x8FC7, 0x8FD8, 0x539F, 0xFF09, 0x3002)
$msgFullBuild = Get-Text @(0x672A, 0x68C0, 0x6D4B, 0x5230, 0x8FD8, 0x539F, 0x7ED3, 0x679C, 0xFF0C, 0x672C, 0x6B21, 0x4F1A, 0x5148, 0x6267, 0x884C, 0x8FD8, 0x539F, 0x518D, 0x7F16, 0x8BD1, 0x3002)
$msgStartBuild = Get-Text @(0x5F00, 0x59CB, 0x7F16, 0x8BD1, 0x0020, 0x0078, 0x0036, 0x0034, 0xFF0C, 0x914D, 0x7F6E, 0xFF1A)
$msgBuildFailed = Get-Text @(0x7F16, 0x8BD1, 0x5931, 0x8D25, 0xFF0C, 0x9000, 0x51FA, 0x7801, 0xFF1A)
$msgBuildSucceeded = Get-Text @(0x7F16, 0x8BD1, 0x6210, 0x529F, 0x3002)
$msgOutputPath = Get-Text @(0x8F93, 0x51FA, 0x76EE, 0x5F55, 0xFF1A)

if (-not (Test-Path $projectPath)) {
    Write-Host ($msgProjectMissing + $projectPath)
    exit 1
}

$arguments = @(
    "build"
    $projectPath
    "-c"
    $Configuration
    "-p:Platform=x64"
    "-p:RuntimeIdentifier=win-x64"
    "--nologo"
    "/m"
)

if ($NoRestore -or (Test-Path $assetsPath)) {
    $arguments += "--no-restore"
    Write-Host $msgFastBuild
}
else {
    Write-Host $msgFullBuild
}

Write-Host ($msgStartBuild + $Configuration)
& dotnet @arguments

if ($LASTEXITCODE -ne 0) {
    Write-Host ($msgBuildFailed + $LASTEXITCODE)
    exit $LASTEXITCODE
}

Write-Host $msgBuildSucceeded
Write-Host ($msgOutputPath + $outputPath)
