<#
.SYNOPSIS
  Builds the React SPA, copies it into the API's wwwroot (without
  touching wwwroot/uploads), and publishes the ASP.NET Core app.

.DESCRIPTION
  Run this from the repo root on your Windows dev machine before each
  deployment. Output lands in .\publish - that folder's contents are
  what actually get uploaded to Plesk (see the deployment guide).

  Uses robocopy /XD uploads specifically so this can be run repeatedly
  against a publish folder that might already contain real uploaded
  photos (e.g. if you're publishing to a folder that mirrors the
  server) without ever deleting them.
#>

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$webDir = Join-Path $repoRoot "apps\web"
$apiDir = Join-Path $repoRoot "apps\api\Portfolio.Api"
$apiWwwroot = Join-Path $apiDir "wwwroot"
$publishDir = Join-Path $repoRoot "publish"

Write-Host "==> Building frontend..." -ForegroundColor Cyan
Push-Location $webDir
npm install
npm run build
Pop-Location

Write-Host "==> Copying built SPA into apps/api/Portfolio.Api/wwwroot (uploads untouched)..." -ForegroundColor Cyan
$webDist = Join-Path $webDir "dist"
# /E copies subfolders including empty ones, /XD excludes the uploads
# directory entirely so real uploaded photos already there are never
# touched, overwritten, or deleted by this step.
robocopy $webDist $apiWwwroot /E /XD uploads /NFL /NDL /NJH /NJS
if ($LASTEXITCODE -ge 8) {
    # Robocopy's exit codes are a bitmask where 0-7 mean success with
    # varying detail; 8+ means a real failure.
    throw "robocopy failed with exit code $LASTEXITCODE"
}

Write-Host "==> Publishing backend..." -ForegroundColor Cyan
if (Test-Path $publishDir) {
    # Only remove previously-published app files, never wwwroot/uploads -
    # dotnet publish will regenerate everything else fresh.
    Remove-Item -Recurse -Force $publishDir
}
dotnet publish $apiDir --configuration Release --output $publishDir

Write-Host "==> Done. Contents of '$publishDir' are what you upload to Plesk." -ForegroundColor Green
Write-Host "    See docs/deployment/plesk-deployment-guide.md for the rest." -ForegroundColor Green
