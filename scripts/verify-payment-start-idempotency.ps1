[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^https://')]
    [string]$StartUrl,

    [Parameter(Mandatory)]
    [string]$BearerToken,

    [Parameter(Mandatory)]
    [string]$PayloadJson,

    [ValidateRange(2, 20)]
    [int]$Concurrency = 10,

    [Guid]$IdempotencyKey = [Guid]::NewGuid(),

    [switch]$ConfirmStaging
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $ConfirmStaging) {
    throw 'This script starts one real staging checkout. Use a disposable target and pass -ConfirmStaging.'
}

if ([string]::IsNullOrWhiteSpace($BearerToken) -or [string]::IsNullOrWhiteSpace($PayloadJson)) {
    throw 'BearerToken and PayloadJson are required.'
}

try {
    $null = $PayloadJson | ConvertFrom-Json
}
catch {
    throw 'PayloadJson must be valid JSON.'
}

$sharedHeaders = @{
    Authorization = "Bearer $BearerToken"
    'Idempotency-Key' = $IdempotencyKey.ToString('D')
    Accept = 'application/json'
}

# Every request has the same key and payload. The expected durable outcome is
# one Payment row and the same PaymentId in all successful API responses.
$results = 1..$Concurrency | ForEach-Object -Parallel {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest `
            -Uri $using:StartUrl `
            -Method Post `
            -Headers $using:sharedHeaders `
            -ContentType 'application/json' `
            -Body $using:PayloadJson `
            -SkipHttpErrorCheck `
            -TimeoutSec 30
        $body = $response.Content | ConvertFrom-Json
        $payment = $body.data
        [pscustomobject]@{
            Request = $_
            StatusCode = [int]$response.StatusCode
            IsSuccess = $body.isSuccess -eq $true
            PaymentId = $payment.paymentId
            DurationMs = $stopwatch.ElapsedMilliseconds
            Error = $null
        }
    }
    catch {
        [pscustomobject]@{
            Request = $_
            StatusCode = $null
            IsSuccess = $false
            PaymentId = $null
            DurationMs = $stopwatch.ElapsedMilliseconds
            Error = $_.Exception.Message
        }
    }
} -ThrottleLimit $Concurrency

$results | Sort-Object Request | Format-Table -AutoSize

$failed = @($results | Where-Object { $_.StatusCode -ne 200 -or -not $_.IsSuccess -or -not $_.PaymentId })
$paymentIds = @($results | Where-Object { $_.PaymentId } | Select-Object -ExpandProperty PaymentId -Unique)
if ($failed.Count -gt 0 -or $paymentIds.Count -ne 1) {
    throw 'Idempotency test failed: all requests must succeed and return exactly one shared PaymentId. Do not continue to callback replay; inspect staging logs and the Payments table.'
}

Write-Host "Start retry passed: $Concurrency concurrent requests returned PaymentId $($paymentIds[0])." -ForegroundColor Green
Write-Host 'Use the callback URL belonging to this payment for verify-payment-callback-replay.ps1.' -ForegroundColor Yellow
