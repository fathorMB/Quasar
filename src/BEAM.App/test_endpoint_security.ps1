# Endpoint Security Test Script
$baseUrl = "https://localhost:7160"

Write-Host "=== Endpoint Security Test ===" -ForegroundColor Cyan

# Skip SSL certificate validation for older PowerShell versions
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Token,
        [string]$ExpectedResult,
        [string]$Description
    )
    
    try {
        $headers = @{}
        if ($Token) {
            $headers["Authorization"] = "Bearer $Token"
        }
        
        $response = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -ErrorAction Stop
        $status = $response.StatusCode
    }
    catch {
        $status = $_.Exception.Response.StatusCode.value__
    }
    
    $expected = if ($ExpectedResult -eq "OK") { 200 } elseif ($ExpectedResult -eq "401") { 401 } elseif ($ExpectedResult -eq "403") { 403 } else { 0 }
    $passed = ($status -eq $expected)
    $color = if ($passed) { "Green" } else { "Red" }
    $result = if ($status -eq 200) { "[OK]" } elseif ($status -eq 401) { "[401]" } elseif ($status -eq 403) { "[403]" } else { "[ERROR:$status]" }
    
    Write-Host "$Description" -NoNewline
    Write-Host " ... " -NoNewline
    Write-Host "$result" -ForegroundColor $color
    
    return $passed
}

Write-Host "Logging in as admin..." -ForegroundColor Yellow
$adminToken = (Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body (@{username = "admin"; password = "ChangeMe123!" } | ConvertTo-Json) -ContentType "application/json").accessToken

Write-Host "Logging in as operator..." -ForegroundColor Yellow
$operatorToken = (Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body (@{username = "operator"; password = "ChangeMe123!" } | ConvertTo-Json) -ContentType "application/json").accessToken

Write-Host ""
Write-Host "=== Testing Identity Endpoints ===" -ForegroundColor Cyan
$results = @()

$results += Test-Endpoint -Method "GET" -Url "$baseUrl/auth/users" -Token $null -ExpectedResult "401" -Description "[Anonymous] GET /auth/users"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/auth/users" -Token $operatorToken -ExpectedResult "403" -Description "[Operator] GET /auth/users"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/auth/users" -Token $adminToken -ExpectedResult "OK" -Description "[Admin] GET /auth/users"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/auth/roles" -Token $operatorToken -ExpectedResult "403" -Description "[Operator] GET /auth/roles"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/auth/acl" -Token $operatorToken -ExpectedResult "403" -Description "[Operator] GET /auth/acl"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/auth/sessions" -Token $operatorToken -ExpectedResult "403" -Description "[Operator] GET /auth/sessions"

Write-Host ""
Write-Host "=== Testing Quartz Endpoints ===" -ForegroundColor Cyan
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/quartz/jobs" -Token $null -ExpectedResult "401" -Description "[Anonymous] GET /quartz/jobs"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/quartz/jobs" -Token $operatorToken -ExpectedResult "OK" -Description "[Operator] GET /quartz/jobs"

Write-Host ""
Write-Host "=== Testing Other Endpoints ===" -ForegroundColor Cyan
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/logs/recent" -Token $null -ExpectedResult "401" -Description "[Anonymous] GET /logs/recent"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/api/features" -Token $null -ExpectedResult "401" -Description "[Anonymous] GET /api/features"
$results += Test-Endpoint -Method "GET" -Url "$baseUrl/api/config/ui" -Token $null -ExpectedResult "OK" -Description "[Anonymous] GET /api/config/ui"

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
$passed = ($results | Where-Object { $_ -eq $true }).Count
$failed = ($results | Where-Object { $_ -eq $false }).Count

Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })

if ($failed -eq 0) {
    Write-Host "[PASS] All tests passed!" -ForegroundColor Green
}
else {
    Write-Host "[FAIL] Some tests failed" -ForegroundColor Red
}
