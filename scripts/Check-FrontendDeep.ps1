[CmdletBinding()]
param(
    [string]$ServerHost,
    [string]$ServerUser
)

$ErrorActionPreference = 'Stop'
$ScriptsDir = $PSScriptRoot
$ConfigPath = Join-Path $ScriptsDir '.env.deploy'
$CredPath   = Join-Path $ScriptsDir '.deploy-credential.xml'

function Resolve-Executable {
    param([string]$Name, [string[]]$Fallbacks)
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    foreach ($candidate in $Fallbacks) {
        if (Test-Path -LiteralPath $candidate) { return $candidate }
    }
    return $null
}

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

$plinkExe = Resolve-Executable -Name 'plink' -Fallbacks @('C:\Program Files\PuTTY\plink.exe')
$securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString
$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-deepcheck-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

$remoteScript = @'
cd /root/pastil_app
echo "===DISK-CHECK==="
test -f frontend/.output/server/node_modules/@iconify/utils/package.json && echo "DISK_HAS_ICONIFY_UTILS=yes" || echo "DISK_HAS_ICONIFY_UTILS=no"
ls -la frontend/.output/server/node_modules/@iconify/ 2>&1
echo "===OUTPUT-DIR-TIMESTAMP==="
stat -c '%y %n' frontend/.output 2>&1
echo "===BUILT-IMAGE-CHECK==="
IMG=$(docker compose images frontend -q 2>/dev/null | head -1)
echo "current_image_id=$IMG"
docker run --rm --entrypoint sh "$IMG" -c "test -f /app/.output/server/node_modules/@iconify/utils/package.json && echo IMAGE_HAS_ICONIFY_UTILS=yes || echo IMAGE_HAS_ICONIFY_UTILS=no"
echo "===IMAGE-BUILD-TIME==="
docker inspect "$IMG" --format '{{.Created}}'
echo "===DOCKERIGNORE-CHECK==="
cat frontend/.dockerignore 2>&1 || echo "no .dockerignore"
'@

try {
    $output = & $plinkExe -batch -ssh -l $ServerUser -pwfile $pwFile $ServerHost $remoteScript 2>&1
    $output | ForEach-Object { Write-Output $_ }
} finally {
    if (Test-Path -LiteralPath $pwFile) {
        try { [System.IO.File]::WriteAllText($pwFile, ('0' * 256)) } catch { }
        Remove-Item -LiteralPath $pwFile -Force -ErrorAction SilentlyContinue
    }
}
