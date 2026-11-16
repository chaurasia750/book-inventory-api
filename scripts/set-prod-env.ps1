<#
.SYNOPSIS
  Sets every secret the API needs on the production IIS server, as environment variables of the application pool.
.DESCRIPTION
  Run once on the server in an elevated (Administrator) PowerShell. The values are written to the application pool in
  applicationHost.config, so a Visual Studio publish (which replaces web.config) never removes them. Nothing is written
  to a file of this repository and no value is printed except the generated passwords, once, at the end: keep them.
  Keys are random. Running it again replaces them, which makes already stored PAN / bank account data unreadable, so
  pass -KeepKeys on every later run (existing Pan / bank / JWT keys are then left alone).
  After the first sign-in of the Super Admin, remove the Seed__* variables (-RemoveSeeds).
.EXAMPLE
  ./set-prod-env.ps1 -PgPassword 'db-password' -SuperAdminEmail 'owner@example.com'.EXAMPLE
  ./set-prod-env.ps1 -RemoveSeeds
#>
param(
    [string] $Pool = 'bi.bankatm.co.in(domain)(4.0)(pool)',
    [string] $PgPassword,
    [string] $PgHost = 'localhost',
    [int] $PgPort = 5432,
    [string] $PgUser = 'BookInventory',
    [string] $Database = 'Live_BookInventory',
    [string] $SuperAdminEmail,
    [switch] $KeepKeys,
    [switch] $RemoveSeeds
)

$ErrorActionPreference = 'Stop'
Import-Module WebAdministration

$filter = "/system.applicationHost/applicationPools/add[@name='$Pool']/environmentVariables"

function Test-PoolVariable([string] $name) {
    [bool](Get-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter "$filter/add[@name='$name']" -Name value -ErrorAction SilentlyContinue)
}

function Remove-PoolVariable([string] $name) {
    if (Test-PoolVariable $name) {
        Remove-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter $filter -Name '.' -AtElement @{ name = $name }
    }
}

function Set-PoolVariable([string] $name, [string] $value) {
    Remove-PoolVariable $name
    Add-WebConfigurationProperty -PSPath MACHINE/WEBROOT/APPHOST -Filter $filter -Name '.' -Value @{ name = $name; value = $value }
}

# Works in Windows PowerShell 5.1 (.NET Framework) and in PowerShell 7: the static GetBytes(int) exists only in .NET Core.
function New-RandomBytes([int] $count) {
    $bytes = New-Object byte[] $count
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $generator.GetBytes($bytes) } finally { $generator.Dispose() }
    $bytes
}

function New-Key([int] $bytes) { [Convert]::ToBase64String((New-RandomBytes $bytes)) }

function New-StrongPassword {
    $core = [Convert]::ToBase64String((New-RandomBytes 18)) -replace '[^A-Za-z0-9]', 'x'
    "Aa1#$core"
}

if (-not (Test-Path "IIS:\AppPools\$Pool")) { throw "Application pool '$Pool' not found. Pass -Pool with the exact name from IIS Manager." }

$seedNames = @('Seed__SuperAdmin__UserName', 'Seed__SuperAdmin__Email', 'Seed__SuperAdmin__FullName', 'Seed__SuperAdmin__Password')

if ($RemoveSeeds) {
    $seedNames | ForEach-Object { Remove-PoolVariable $_ }
    "Seed variables removed from pool '$Pool'. Recycle only this pool to apply: Restart-WebAppPool -Name '$Pool' (not iisreset: other sites share the server)."
    return
}

if (-not $PgPassword -or -not $SuperAdminEmail) { throw 'Pass -PgPassword and -SuperAdminEmail (or -RemoveSeeds).' }

Set-PoolVariable 'ConnectionStrings__Default' "Host=$PgHost;Port=$PgPort;Database=$Database;Username=$PgUser;Password=`"$PgPassword`""

foreach ($entry in @(
        @{ Name = 'JwtSetting__IssuerSigningKey'; Bytes = 48 },
        @{ Name = 'Pan__EncryptionKey'; Bytes = 32 },
        @{ Name = 'Pan__FingerprintKey'; Bytes = 32 },
        @{ Name = 'Finance__BankAccount__EncryptionKey'; Bytes = 32 })) {
    if ($KeepKeys -and (Test-PoolVariable $entry.Name)) { continue }
    Set-PoolVariable $entry.Name (New-Key $entry.Bytes)
}

$superAdmin = New-StrongPassword
$systemSecret = New-StrongPassword
Set-PoolVariable 'Seed__SuperAdmin__UserName' 'superadmin'
Set-PoolVariable 'Seed__SuperAdmin__Email' $SuperAdminEmail
Set-PoolVariable 'Seed__SuperAdmin__FullName' 'Book Center Admin'
Set-PoolVariable 'Seed__SuperAdmin__Password' $superAdmin
if (-not $KeepKeys -or -not (Test-PoolVariable 'SystemAccount__Password')) { Set-PoolVariable 'SystemAccount__Password' $systemSecret }

"Variables set on pool '$Pool'. Recycle only this pool to apply: Restart-WebAppPool -Name '$Pool' (not iisreset: other sites share the server)."
"Back up Pan__EncryptionKey, Pan__FingerprintKey and Finance__BankAccount__EncryptionKey: if they are lost, stored PAN and bank numbers cannot be read."
"superadmin       : $superAdmin   (first sign-in forces a password change)"
"system account   : $systemSecret"
"Next: Release_Checklist.md; change the Seed values to real ones in IIS before the first start if needed, then run with -RemoveSeeds after the first sign-ins."
