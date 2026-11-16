<#
.SYNOPSIS
  Sets every user-secret the API needs on a developer machine, with fresh random values.
.DESCRIPTION
  Run once after cloning. It never writes to a committed file: the values go to `dotnet user-secrets` of src/IngoTech.Api
  (stored under %APPDATA%\Microsoft\UserSecrets). Generated passwords are printed once at the end; keep them.
  Running it again replaces the keys, which makes already stored PAN / bank account data of that database unreadable,
  so use it on a fresh database (or pass -KeepKeys to leave existing keys alone).
.EXAMPLE
  ./scripts/setup-dev-secrets.ps1 -PgPassword 'your-local-postgres-password' -WithDemoMembers
#>
param(
    [Parameter(Mandatory)] [string] $PgPassword,
    [string] $PgHost = 'localhost',
    [int] $PgPort = 5432,
    [string] $PgUser = 'postgres',
    [string] $Database = 'ingo_tech',
    [switch] $WithDemoMembers,
    [switch] $KeepKeys
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\src\IngoTech.Api'

function New-Key([int] $bytes) { [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes($bytes)) }

function New-StrongPassword {
    $core = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(12)) -replace '[^A-Za-z0-9]', 'x'
    "Aa1#$core"
}

function Set-Secret([string] $key, [string] $value) {
    dotnet user-secrets set $key $value --project $project | Out-Null
}

function Test-HasSecret([string] $key) {
    [bool](dotnet user-secrets list --project $project | Select-String -SimpleMatch "$key =")
}

Set-Secret 'ConnectionStrings:Default' "Host=$PgHost;Port=$PgPort;Database=$Database;Username=$PgUser;Password=$PgPassword"

foreach ($entry in @(
        @{ Key = 'JwtSetting:IssuerSigningKey'; Bytes = 48 },
        @{ Key = 'Pan:EncryptionKey'; Bytes = 32 },
        @{ Key = 'Pan:FingerprintKey'; Bytes = 32 },
        @{ Key = 'Finance:BankAccount:EncryptionKey'; Bytes = 32 })) {
    if ($KeepKeys -and (Test-HasSecret $entry.Key)) { continue }
    Set-Secret $entry.Key (New-Key $entry.Bytes)
}

$superAdmin = New-StrongPassword
$root = New-StrongPassword
Set-Secret 'Seed:SuperAdmin:Password' $superAdmin
Set-Secret 'Seed:RootMember:Password' $root
Set-Secret 'Seed:RootMember:MemberCode' 'IT100001'
Set-Secret 'Seed:RootMember:FullName' 'Ingo Tech Root'
Set-Secret 'Seed:RootMember:Email' 'root@ingo-tech.local'
Set-Secret 'Seed:RootMember:Mobile' '9000000011'

$demo = $null
if ($WithDemoMembers) {
    $demo = New-StrongPassword
    Set-Secret 'Seed:DemoMembers:Password' $demo
    $people = @(@('First', 'IT100002', 'Demo Member A', 'demo.a', '9000000012'),
                @('Second', 'IT100003', 'Demo Member B', 'demo.b', '9000000013'),
                @('Third', 'IT100004', 'Demo Member C', 'demo.c', '9000000014'))
    foreach ($p in $people) {
        Set-Secret "Seed:DemoMembers:$($p[0]):MemberCode" $p[1]
        Set-Secret "Seed:DemoMembers:$($p[0]):FullName" $p[2]
        Set-Secret "Seed:DemoMembers:$($p[0]):Email" "$($p[3])@ingo-tech.local"
        Set-Secret "Seed:DemoMembers:$($p[0]):Mobile" $p[4]
    }
}

"Secrets are set for $project"
"Database          : $Database on ${PgHost}:$PgPort (create it first: CREATE DATABASE $Database;)"
"superadmin        : $superAdmin   (first sign-in forces a password change)"
"IT100001 (root)   : $root   (first sign-in forces a password change)"
if ($demo) { "IT100002-4 (demo) : $demo" }
"Next: see docs/Setup.md"
