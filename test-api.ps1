# =============================================================================
# E-Commerce API - FINAL TEST SUITE (PS5 compatible)
# =============================================================================

param(
  [string]$BaseUrl = "http://localhost:5164",
  [string]$ApiVersion = "1"
)

function Section($t) {
  Write-Host "`n============================================================" -ForegroundColor Cyan
  Write-Host " $t" -ForegroundColor Cyan
  Write-Host "============================================================" -ForegroundColor Cyan
}

Write-Host "STARTING TEST SUITE..." -ForegroundColor Green

# 1) HEALTH
Section "HEALTH CHECK TEST"
try {
  $r = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -ErrorAction Stop
  if ($r.StatusCode -eq 200) {
    Write-Host "Health Check successful. Status: 200" -ForegroundColor Green
  } else {
    Write-Host "Health Check failed. Status: $($r.StatusCode)" -ForegroundColor Red
  }
} catch {
  Write-Host "Health Check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# 2) CURSOR PAGINATION  (VERSİYONLU ROUTE)
Section "CURSOR PAGINATION TEST"
try {
  $r1 = Invoke-WebRequest -Uri "$BaseUrl/api/v$ApiVersion/products?pageSize=2&useCursor=true" -UseBasicParsing -ErrorAction Stop
  $j1 = $r1.Content | ConvertFrom-Json
  if ($j1.items.Count -gt 0) {
    Write-Host ("Page 1 - First item ID: {0}" -f $j1.items[0].id)
  } else {
    Write-Host "Page 1 - No items" -ForegroundColor Yellow
  }

  $cursor = $j1.nextCursor
  Write-Host "Page 1 - Next cursor: $cursor"

  if ($cursor) {
    $r2 = Invoke-WebRequest -Uri "$BaseUrl/api/v$ApiVersion/products?pageSize=2&useCursor=true&cursor=$cursor" -UseBasicParsing -ErrorAction Stop
    $j2 = $r2.Content | ConvertFrom-Json
    if ($j2.items.Count -gt 0) {
      Write-Host "Cursor test OK: Page 2 has items." -ForegroundColor Green
    } else {
      Write-Host "Cursor test OK: Page 2 empty (no more data)." -ForegroundColor Green
    }
  } else {
    Write-Host "Cursor test inconclusive: Not enough products to page." -ForegroundColor Yellow
  }
}
catch {
  $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { "N/A" }
  Write-Host "Cursor test FAILED with status '$code': $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nWaiting 11 seconds for rate limit window to reset..." -ForegroundColor Yellow
Start-Sleep -Seconds 11

# 3) RATE LIMITING  (VERSİYONLU ROUTE, 7 istek)
Section "RATE LIMITING TEST"
$limitHit = $false
for ($i = 1; $i -le 7; $i++) {
  Write-Host ("Request {0} ..." -f $i) -NoNewline
  try {
    $r = Invoke-WebRequest -Uri "$BaseUrl/api/v$ApiVersion/products" -UseBasicParsing -ErrorAction Stop
    Write-Host (" -> SUCCESS (Code: {0})" -f $r.StatusCode) -ForegroundColor Green
  } catch {
    $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { "N/A" }
    if ($code -eq 429) {
      Write-Host " -> RATE LIMITED (Code: 429)" -ForegroundColor Yellow
      $limitHit = $true
    } else {
      Write-Host (" -> FAILED (Code: {0})" -f $code) -ForegroundColor Red
    }
  }
  Start-Sleep -Milliseconds 100
}
if ($limitHit) {
  Write-Host "Rate limiting PASSED." -ForegroundColor Green
} else {
  Write-Host "Rate limiting FAILED (no 429 seen)." -ForegroundColor Red
}

Write-Host "`nWaiting 11 seconds for rate limit window to reset..." -ForegroundColor Yellow
Start-Sleep -Seconds 11

# 4) LOCALIZATION (404 + tr-TR)
Section "LOCALIZATION TEST"
try {
  $headers = @{ "Accept-Language" = "tr-TR" }
  Invoke-WebRequest -Uri "$BaseUrl/api/v$ApiVersion/products/00000000-0000-0000-0000-000000000000" -Headers $headers -UseBasicParsing -ErrorAction Stop
} catch {
  $code = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { "N/A" }
  if ($code -eq 404) {
    Write-Host "Localization OK: 404 dondu (tr-TR istegi)." -ForegroundColor Green
  } else {
    Write-Host "Localization FAILED. Expected 404 but got $code." -ForegroundColor Red
  }
}

Write-Host "`n TEST SUITE COMPLETED!" -ForegroundColor Green
