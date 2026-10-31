param(
    [string]$OutputDirectory = "artifacts/release"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$webDirectory = Join-Path $repositoryRoot "apps/web"
$apiDirectory = Join-Path $repositoryRoot "apps/api"
$artifactsDirectory = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts"))
$publishDirectory = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
$allowedPrefix = $artifactsDirectory.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar

if (-not $publishDirectory.StartsWith($allowedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDirectory must stay inside $artifactsDirectory."
}

# A clean artifact prevents old React hashed assets and stale assemblies
# from leaking into a release. The resolved path is checked above before
# this recursive deletion runs.
if (Test-Path -LiteralPath $publishDirectory) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

Push-Location $webDirectory
try {
    npm.cmd ci
    if ($LASTEXITCODE -ne 0) { throw "npm ci failed with exit code $LASTEXITCODE." }
    npm.cmd run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build failed with exit code $LASTEXITCODE." }
}
finally {
    Pop-Location
}

dotnet publish (Join-Path $apiDirectory "Portfolio.Api/Portfolio.Api.csproj") -c Release -o $publishDirectory
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }
New-Item -ItemType Directory -Path (Join-Path $publishDirectory "wwwroot") -Force | Out-Null
Copy-Item (Join-Path $webDirectory "dist/*") (Join-Path $publishDirectory "wwwroot") -Recurse -Force

# Initial public gallery variants are intentionally tracked starter media.
# They do not go inside wwwroot on the server. Copy initial-media/* into
# the persistent PhotoStorage:RootPath before the first app startup.
$initialMediaSource = Join-Path $apiDirectory "Portfolio.Api/wwwroot/uploads"
$initialMediaDestination = Join-Path $publishDirectory "initial-media"
if (Test-Path -LiteralPath $initialMediaSource) {
    New-Item -ItemType Directory -Path $initialMediaDestination -Force | Out-Null
    Copy-Item (Join-Path $initialMediaSource "*") $initialMediaDestination -Recurse -Force
}

Write-Host "Release artifact prepared at $publishDirectory"
