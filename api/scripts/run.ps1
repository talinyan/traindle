$ErrorActionPreference = 'Stop'
Push-Location "../api"
try {
    dotnet watch --launch-profile https
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
