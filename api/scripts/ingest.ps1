$ErrorActionPreference = 'Stop'
Push-Location "../api"
try {
    dotnet run --launch-profile http -- 'ingest'
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
