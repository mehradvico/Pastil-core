[CmdletBinding()]
param([string]$ServerHost, [string]$ServerUser)
$ErrorActionPreference = 'Stop'
$ScriptsDir = $PSScriptRoot
$ConfigPath = Join-Path $ScriptsDir '.env.deploy'
$CredPath   = Join-Path $ScriptsDir '.deploy-credential.xml'
$config = @{}
if (Test-Path -LiteralPath $ConfigPath) {
    foreach ($line in (Get-Content -LiteralPath $ConfigPath)) {
        $t = $line.Trim(); if (-not $t -or $t.StartsWith('#')) { continue }
        $sep = $t.IndexOf('='); if ($sep -le 0) { continue }
        $config[$t.Substring(0,$sep).Trim()] = $t.Substring($sep+1).Trim().Trim('"').Trim("'")
    }
}
if (-not $ServerHost) { $ServerHost = $config['PASTIL_DEPLOY_HOST'] }
if (-not $ServerUser) { $ServerUser = $config['PASTIL_DEPLOY_USER'] }
if (-not $ServerUser) { $ServerUser = 'root' }
$plinkExe = 'C:\Program Files\PuTTY\plink.exe'
$securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString
$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-diskq-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $p = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $p, (New-Object System.Text.UTF8Encoding($false)))
} finally { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
try {
    & $plinkExe -batch -ssh -l $ServerUser -pwfile $pwFile $ServerHost "df -h /" 2>&1
} finally {
    if (Test-Path -LiteralPath $pwFile) { [System.IO.File]::WriteAllText($pwFile,('0'*256)); Remove-Item -LiteralPath $pwFile -Force -ErrorAction SilentlyContinue }
}
