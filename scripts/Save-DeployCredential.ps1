[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# Codex and some developer shells prepend PowerShell 7 compatibility modules
# to PSModulePath. Windows PowerShell 5.1 then discovers those incompatible
# modules before its own inbox modules and cannot autoload Security cmdlets.
$env:PSModulePath = @(
    (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'WindowsPowerShell\Modules')
    (Join-Path $env:ProgramFiles 'WindowsPowerShell\Modules')
    (Join-Path $PSHOME 'Modules')
) -join ';'

$credentialPath = Join-Path $PSScriptRoot '.deploy-credential.xml'

Write-Host ''
Write-Host '  Pastil Deploy - SSH credential' -ForegroundColor Cyan
Write-Host '  The password is encrypted for this Windows user and machine.' -ForegroundColor DarkGray
Write-Host ''

$securePassword = Read-Host -Prompt '  SSH password' -AsSecureString
if ($securePassword.Length -eq 0) {
    Write-Host ''
    Write-Host '  Password was empty. Nothing was changed.' -ForegroundColor Yellow
    exit 1
}

$encrypted = $securePassword | ConvertFrom-SecureString
$encrypted | Set-Content -LiteralPath $credentialPath -Encoding utf8

# Verify that the freshly written value is readable by this user before
# reporting success. The plaintext password is never materialized.
$null = (Get-Content -LiteralPath $credentialPath -Raw).Trim() |
    ConvertTo-SecureString -ErrorAction Stop

Write-Host ''
Write-Host '  Credential saved successfully.' -ForegroundColor Green
Write-Host '  Monitoring will reconnect automatically.' -ForegroundColor Green
Write-Host ''
