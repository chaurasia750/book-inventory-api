<#
.SYNOPSIS
  Builds the three Angular apps and publishes the API with them in its wwwroot, ready for IIS (Plan 0010).
.DESCRIPTION
  1. builds public-site (/), admin (/admin/) and member (/member/) with the production configuration in the UI repository,
  2. publishes the API (Release) to -Output,
  3. copies the apps into <Output>\wwwroot (public site at the root, the others in their folders) and drops the
     per-app web.config files, which the ASP.NET Core host does not use.
  The API serves wwwroot only when it exists, so a normal `dotnet run` never serves the UI (the UI runs on its own dev server).
  -Output must not exist or must be empty unless -Clean is given, so a stale file can never be published by accident.
#>
param(
    [string]$Output = (Join-Path $PSScriptRoot '..\publish\book-inventory'),
    [string]$UiPath = (Join-Path $PSScriptRoot '..\..\book-inventory-ui'),
    [switch]$SkipUiBuild,
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$repo = Resolve-Path (Join-Path $PSScriptRoot '..')
$ui = Resolve-Path $UiPath
$apps = @(
    @{ Name = 'public-site'; BaseHref = '/';        Target = 'wwwroot' },
    @{ Name = 'admin';       BaseHref = '/admin/';  Target = 'wwwroot\admin' },
    @{ Name = 'member';      BaseHref = '/member/'; Target = 'wwwroot\member' }
)

if ((Test-Path $Output) -and (Get-ChildItem $Output -Force | Select-Object -First 1)) {
    if (-not $Clean) { throw "Output folder '$Output' is not empty. Use -Clean to replace its content." }
    Get-ChildItem $Output -Force | Remove-Item -Recurse -Force
}

if (-not $SkipUiBuild) {
    Push-Location $ui
    try {
        foreach ($app in $apps) {
            Write-Host "== building $($app.Name) ($($app.BaseHref))"
            & npx nx build $app.Name --configuration=production "--base-href=$($app.BaseHref)"
            if ($LASTEXITCODE -ne 0) { throw "UI build of $($app.Name) failed" }
        }
    } finally { Pop-Location }
}

Write-Host '== publishing the API'
& dotnet publish (Join-Path $repo 'src\BookInventory.Api\BookInventory.Api.csproj') -c Release -o $Output
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

foreach ($app in $apps) {
    $source = Join-Path $ui "dist\apps\$($app.Name)\browser"
    if (-not (Test-Path (Join-Path $source 'index.html'))) { throw "No build found in $source" }
    $target = Join-Path $Output $app.Target
    New-Item -ItemType Directory -Force $target | Out-Null
    Copy-Item (Join-Path $source '*') $target -Recurse -Force
}
Get-ChildItem (Join-Path $Output 'wwwroot') -Recurse -Filter web.config | Remove-Item -Force

Write-Host "== done: $Output"
Write-Host 'Set the secrets as environment variables (docs/Release_Checklist.md), then run: dotnet BookInventory.Api.dll'
