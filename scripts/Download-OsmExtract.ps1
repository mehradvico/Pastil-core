[CmdletBinding()]
param(
    [string]$ServerHost,
    [string]$ServerUser
)

$ErrorActionPreference = 'Stop'
$ScriptsDir = $PSScriptRoot
$ConfigPath = Join-Path $ScriptsDir '.env.deploy'
$CredPath   = Join-Path $ScriptsDir '.deploy-credential.xml'

$config = @{}
if (Test-Path -LiteralPath $ConfigPath) {
    foreach ($line in (Get-Content -LiteralPath $ConfigPath)) {
        $trimmed = $line.Trim()
        if (-not $trimmed -or $trimmed.StartsWith('#')) { continue }
        $separator = $trimmed.IndexOf('=')
        if ($separator -le 0) { continue }
        $key = $trimmed.Substring(0, $separator).Trim()
        $value = $trimmed.Substring($separator + 1).Trim().Trim('"').Trim("'")
        $config[$key] = $value
    }
}
if (-not $ServerHost) { $ServerHost = $config['PASTIL_DEPLOY_HOST'] }
if (-not $ServerUser) { $ServerUser = $config['PASTIL_DEPLOY_USER'] }
if (-not $ServerUser) { $ServerUser = 'root' }

$plinkExe = 'C:\Program Files\PuTTY\plink.exe'
$securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString
$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-osmdl-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

try {
    Write-Host "Downloading Iran OSM extract on the server via openstreetmap.fr mirror..." -ForegroundColor Cyan
    $remoteScript = @'
mkdir -p /root/osm-build
cd /root/osm-build
rm -f iran-latest.osm.pbf
echo "--> downloading from openstreetmap.fr mirror"
curl -sS -L --retry 3 -o iran-latest.osm.pbf https://download.openstreetmap.fr/extracts/asia/iran-latest.osm.pbf
echo "--> done, checking result"
ls -lh iran-latest.osm.pbf
file iran-latest.osm.pbf
'@
    $remoteScript = $remoteScript -replace "`r`n", "`n"

    $prevPref = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'
    $output = & $plinkExe -batch -ssh -l $ServerUser -pwfile $pwFile $ServerHost $remoteScript 2>&1
    $ErrorActionPreference = $prevPref
    $output | ForEach-Object {
        if ($_ -is [System.Management.Automation.ErrorRecord]) { Write-Host $_.Exception.Message } else { Write-Host $_ }
    }
} finally {
    if (Test-Path -LiteralPath $pwFile) {
        try { [System.IO.File]::WriteAllText($pwFile, ('0' * 256)) } catch { }
        Remove-Item -LiteralPath $pwFile -Force -ErrorAction SilentlyContinue
    }
}
