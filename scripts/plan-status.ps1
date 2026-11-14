<#
.SYNOPSIS
  Shows how many plans are done and which are pending, read from the plan files themselves (no AI needed).
.DESCRIPTION
  Reads the header of every plan in docs/plans/<module>/*.md (Status, Owner, Depends on, Kind) and prints:
  1. every plan with its status, owner and dependencies, oldest first per module,
  2. a summary per module: done / pending / total and the first pending plan number,
  3. the plans that can start now (Approved, and every plan they depend on is Done).
  The plan header is the only source: nothing is counted or stored anywhere else.
.EXAMPLE
  ./scripts/plan-status.ps1
  ./scripts/plan-status.ps1 -Module iam
#>
param(
    [string]$Module
)

$ErrorActionPreference = 'Stop'
$plansRoot = Join-Path $PSScriptRoot '..\docs\plans'

function Read-HeaderField([string[]]$lines, [string]$name) {
    $line = $lines | Where-Object { $_ -match "^- \*\*$name\:\*\*" } | Select-Object -First 1
    if (-not $line) { return '' }
    return ($line -replace "^- \*\*$name\:\*\*\s*", '').Trim()
}

function Read-Plan([System.IO.FileInfo]$file) {
    $lines = Get-Content -LiteralPath $file.FullName -TotalCount 15
    $moduleName = $file.Directory.Name
    $number = ($file.BaseName -split '-')[3]
    $status = (Read-HeaderField $lines 'Status') -replace '\s*\(.*$', ''
    $depends = (Read-HeaderField $lines 'Depends on') -replace '`', ''
    [pscustomobject]@{
        Module    = $moduleName
        Id        = "$moduleName#$number"
        Status    = $status
        Kind      = Read-HeaderField $lines 'Kind'
        Owner     = Read-HeaderField $lines 'Owner'
        DependsOn = if ($depends -eq 'none' -or $depends -eq '') { @() } else { @($depends -split ',\s*') }
        Title     = ($lines[0] -replace '^# Plan [^:]*:\s*', '')
    }
}

$moduleDirs = Get-ChildItem -LiteralPath $plansRoot -Directory | Sort-Object Name
if ($Module) { $moduleDirs = $moduleDirs | Where-Object Name -eq $Module }
if (-not $moduleDirs) { Write-Host 'No module plan folders found.'; return }

$plans = foreach ($dir in $moduleDirs) {
    Get-ChildItem -LiteralPath $dir.FullName -Filter '*.md' | Sort-Object Name | ForEach-Object { Read-Plan $_ }
}
if (-not $plans) { Write-Host 'No plans yet.'; return }

Write-Host "`nPlans" -ForegroundColor Cyan
$plans | Format-Table Id, Status, Kind, Owner, @{ Label = 'Depends on'; Expression = { $_.DependsOn -join ', ' } }, Title -AutoSize | Out-Host

Write-Host 'Summary per module' -ForegroundColor Cyan
$plans | Group-Object Module | ForEach-Object {
    $done = @($_.Group | Where-Object Status -eq 'Done').Count
    $pending = @($_.Group | Where-Object { $_.Status -ne 'Done' -and $_.Status -notlike 'Superseded*' })
    [pscustomobject]@{
        Module          = $_.Name
        Done            = $done
        Pending         = $pending.Count
        Total           = $_.Count
        'First pending' = if ($pending) { $pending[0].Id } else { '-' }
    }
} | Format-Table -AutoSize | Out-Host

$doneIds = @($plans | Where-Object Status -eq 'Done' | ForEach-Object Id)
$ready = $plans | Where-Object {
    $_.Status -eq 'Approved' -and -not ($_.DependsOn | Where-Object { $_ -notin $doneIds })
}
Write-Host 'Can start now (Approved, dependencies Done)' -ForegroundColor Cyan
if ($ready) { $ready | Format-Table Id, Owner, Title -AutoSize | Out-Host } else { Write-Host "  none`n" }

$inProgress = $plans | Where-Object Status -eq 'In progress'
if ($inProgress) {
    Write-Host 'In progress' -ForegroundColor Cyan
    $inProgress | Format-Table Id, Owner, Title -AutoSize | Out-Host
}
