param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$relativeFiles = @(
    'Api/appsettings.json',
    'Api/appsettings.Development.json',
    'Payment/appsettings.json',
    'Payment/appsettings.Development.json',
    'File/appsettings.json',
    'File/appsettings.Development.json',
    'RealTime/appsettings.json',
    'RealTime/appsettings.Development.json',
    'publish-api/appsettings.json',
    'publish-api/appsettings.Development.json',
    'publish-payment/appsettings.json',
    'publish-payment/appsettings.Development.json',
    'publish-file/appsettings.json',
    'publish-file/appsettings.Development.json'
)

$secretProperties = @(
    'connection',
    'key',
    'PublicKey',
    'PrivateKey',
    'ApiKey'
)

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

foreach ($relativeFile in $relativeFiles) {
    $path = Join-Path $RepositoryRoot $relativeFile
    if (-not (Test-Path -LiteralPath $path)) {
        continue
    }

    $content = [System.IO.File]::ReadAllText($path)

    foreach ($property in $secretProperties) {
        $escapedProperty = [Regex]::Escape($property)
        $pattern = '(?m)(^\s*"' + $escapedProperty + '"\s*:\s*)"(?:\\.|[^"\\])*"'
        $content = [Regex]::Replace($content, $pattern, '$1""')
    }

    # Validate before replacing the original file.
    $null = $content | ConvertFrom-Json
    [System.IO.File]::WriteAllText($path, $content, $utf8WithoutBom)

    Write-Output "Sanitized: $relativeFile"
}
