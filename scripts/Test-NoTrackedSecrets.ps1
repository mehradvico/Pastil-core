param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$patterns = [ordered]@{
    'GCP API key' = 'AIza[0-9A-Za-z_-]{30,}'
    'Groq API key' = 'gsk_[0-9A-Za-z_-]{20,}'
    'OpenAI-style API key' = 'sk-[0-9A-Za-z_-]{20,}'
    'Connection-string password' = '(?i)(^|;)\s*(Password|Pwd)\s*=\s*[^;"\s]{3,}'
    'Private-key material' = '-----BEGIN [A-Z ]*PRIVATE KEY-----'
    'Non-empty JSON ApiKey' = '(?i)"ApiKey"\s*:\s*"[^"\s]+"'
    'Non-empty JSON PrivateKey' = '(?i)"PrivateKey"\s*:\s*"[^"\s]+"'
    'Non-empty JSON JWT key' = '(?i)"key"\s*:\s*"[^"\s]+"'
    'Non-empty JSON connection' = '(?i)"connection"\s*:\s*"[^"\s]+"'
}

$textExtensions = @(
    '.cs', '.cshtml', '.json', '.xml', '.config', '.md', '.txt',
    '.ps1', '.psm1', '.props', '.targets', '.csproj', '.sln', '.yml',
    '.yaml', '.js', '.ts', '.tsx', '.jsx', '.html', '.css', '.env', ''
)

$files = & git -C $RepositoryRoot ls-files --cached --others --exclude-standard
$findings = [System.Collections.Generic.List[string]]::new()

foreach ($relativeFile in $files) {
    if ($relativeFile -eq '.env') {
        continue
    }

    $path = Join-Path $RepositoryRoot $relativeFile
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $extension = [System.IO.Path]::GetExtension($path).ToLowerInvariant()
    if ($textExtensions -notcontains $extension) {
        continue
    }

    $fileInfo = Get-Item -LiteralPath $path
    if ($fileInfo.Length -gt 5MB) {
        continue
    }

    $content = [System.IO.File]::ReadAllText($path)
    foreach ($entry in $patterns.GetEnumerator()) {
        if ([Regex]::IsMatch($content, $entry.Value)) {
            $findings.Add("$($entry.Key): $relativeFile")
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Error ("Potential tracked secrets found:`n" + ($findings -join "`n"))
    exit 1
}

Write-Output 'No potential secret was found in tracked or unignored text files.'
