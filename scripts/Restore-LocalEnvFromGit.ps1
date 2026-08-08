param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$Revision = 'HEAD'
)

$ErrorActionPreference = 'Stop'

$environmentPath = Join-Path $RepositoryRoot '.env'
if (-not (Test-Path -LiteralPath $environmentPath)) {
    throw '.env was not found. Copy .env.example to .env first.'
}

function Read-JsonFromGit([string]$relativePath) {
    $content = & git -C $RepositoryRoot show "$Revision`:$relativePath" 2>$null
    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    return $content | ConvertFrom-Json
}

function Set-EnvironmentValue(
    [System.Collections.Generic.List[string]]$lines,
    [string]$key,
    [string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $false
    }

    $prefix = "$key="
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index].StartsWith($prefix, [StringComparison]::Ordinal)) {
            $currentValue = $lines[$index].Substring($prefix.Length)
            if ([string]::IsNullOrWhiteSpace($currentValue)) {
                $lines[$index] = "$prefix$value"
                return $true
            }

            return $false
        }
    }

    $lines.Add("$prefix$value")
    return $true
}

$configs = @{
    Api = Read-JsonFromGit 'Api/appsettings.json'
    Payment = Read-JsonFromGit 'Payment/appsettings.json'
    File = Read-JsonFromGit 'File/appsettings.json'
    RealTime = Read-JsonFromGit 'RealTime/appsettings.json'
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.AddRange([string[]](Get-Content -LiteralPath $environmentPath))
$migratedCount = 0

$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_API_CONNECTION' $configs.Api.connection)
$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_PAYMENT_CONNECTION' $configs.Payment.connection)
$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_FILE_CONNECTION' $configs.File.connection)
$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_REALTIME_CONNECTION' $configs.RealTime.connection)
$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_JWT_KEY' $configs.Api.JWtConfig.key)
$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_VAPID_PUBLIC_KEY' $configs.Api.VapidKeys.PublicKey)
$migratedCount += [int](Set-EnvironmentValue $lines 'PASTIL_VAPID_PRIVATE_KEY' $configs.Api.VapidKeys.PrivateKey)

$providerEnvironmentVariables = @{
    Gemini = 'PASTIL_AI_GEMINI_API_KEY'
    Groq = 'PASTIL_AI_GROQ_API_KEY'
    ChatGPT = 'PASTIL_AI_OPENAI_API_KEY'
    DeepSeek = 'PASTIL_AI_DEEPSEEK_API_KEY'
    AvalAI = 'PASTIL_AI_AVALAI_API_KEY'
    GapGPT = 'PASTIL_AI_GAPGPT_API_KEY'
}

foreach ($provider in $configs.Api.PastilAI.Providers) {
    $environmentVariable = $providerEnvironmentVariables[$provider.Name]
    if ($environmentVariable) {
        $migratedCount += [int](Set-EnvironmentValue $lines $environmentVariable $provider.ApiKey)
    }
}

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllLines($environmentPath, $lines, $utf8WithoutBom)

Write-Output "Migrated local secret values: $migratedCount"
Write-Output 'No secret value was printed. Missing values must be filled manually in .env.'
