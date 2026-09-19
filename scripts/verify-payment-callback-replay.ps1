[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^https://')]
    [string]$CallbackUrl,

    [ValidateRange(2, 30)]
    [int]$Concurrency = 20,

    [switch]$ConfirmStaging
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $ConfirmStaging) {
    throw 'This script replays a real callback URL. Run it only against a disposable staging payment and pass -ConfirmStaging.'
}

$uri = [Uri]$CallbackUrl
if ($uri.Scheme -ne 'https' -or $uri.AbsolutePath -notmatch '/callback/\d+$' -or
    $uri.Query -notmatch '(?:^|[?&])callbackToken=') {
    throw 'CallbackUrl must be an HTTPS /callback/{paymentId} URL that includes callbackToken.'
}

# This script deliberately does not create a payment. Create one through the
# staging UI with PaymentTestMode enabled, then replay only that disposable URL.
$results = 1..$Concurrency | ForEach-Object -Parallel {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $using:CallbackUrl -MaximumRedirection 0 -SkipHttpErrorCheck -TimeoutSec 30
        [pscustomobject]@{
            Request = $_
            StatusCode = [int]$response.StatusCode
            SuccessPage = $response.Content -match 'payment-shell is-success'
            DurationMs = $stopwatch.ElapsedMilliseconds
            Error = $null
        }
    }
    catch {
        [pscustomobject]@{
            Request = $_
            StatusCode = $null
            SuccessPage = $false
            DurationMs = $stopwatch.ElapsedMilliseconds
            Error = $_.Exception.Message
        }
    }
} -ThrottleLimit $Concurrency

$results | Sort-Object Request | Format-Table -AutoSize

$failures = @($results | Where-Object {
    $_.StatusCode -ne 200 -or -not $_.SuccessPage
})

if ($failures.Count -gt 0) {
    throw "$($failures.Count) of $Concurrency replay requests did not receive the successful callback page. Do not reuse this payment; inspect the staging logs and database assertion query."
}

Write-Host "Replay passed: $Concurrency concurrent callbacks returned the successful page." -ForegroundColor Green
Write-Host 'Run scripts/database/AssertWalletPaymentCallbackReplay.sql with this payment ID to prove that the wallet credit was created exactly once.' -ForegroundColor Yellow
