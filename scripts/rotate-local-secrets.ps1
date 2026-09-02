[CmdletBinding()]
param(
    [switch]$RotateRunningServices
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$envPath = Join-Path $repositoryRoot '.env'

function New-RandomSecret([int]$ByteCount = 48) {
    $bytes = [byte[]]::new($ByteCount)
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Set-DotEnvValue([string]$Content, [string]$Name, [string]$Value) {
    $line = "$Name=$Value"
    if ($Content -match "(?m)^$([regex]::Escape($Name))=") {
        return [regex]::Replace($Content, "(?m)^$([regex]::Escape($Name))=.*$", $line)
    }

    if ($Content.Length -gt 0 -and -not $Content.EndsWith("`n")) {
        $Content += "`n"
    }
    return $Content + $line + "`n"
}

$postgresPassword = New-RandomSecret
$rabbitPassword = New-RandomSecret
$jwtKey = New-RandomSecret 64
$postgresUser = 'smarthome'
$postgresDatabase = 'SmartHouse'
$rabbitUser = 'rmuser'

if ($RotateRunningServices) {
    $postgresContainer = docker ps --filter 'label=com.docker.compose.service=postgres' --format '{{.ID}}' | Select-Object -First 1
    $rabbitContainer = docker ps --filter 'label=com.docker.compose.service=rabbitmq' --format '{{.ID}}' | Select-Object -First 1
    if (-not $postgresContainer -or -not $rabbitContainer) {
        throw 'Both the Compose postgres and rabbitmq containers must be running for an in-place rotation.'
    }

    "ALTER ROLE $postgresUser WITH PASSWORD '$postgresPassword';" |
        docker exec -i $postgresContainer psql --set ON_ERROR_STOP=1 --username $postgresUser --dbname $postgresDatabase
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL credential rotation failed.' }

    docker exec $rabbitContainer rabbitmqctl change_password $rabbitUser $rabbitPassword
    if ($LASTEXITCODE -ne 0) { throw 'RabbitMQ credential rotation failed.' }
}

$envContent = if (Test-Path -LiteralPath $envPath) {
    [System.IO.File]::ReadAllText($envPath)
} else {
    ''
}
$envContent = Set-DotEnvValue $envContent 'POSTGRES_DB' $postgresDatabase
$envContent = Set-DotEnvValue $envContent 'POSTGRES_USER' $postgresUser
$envContent = Set-DotEnvValue $envContent 'POSTGRES_PASSWORD' $postgresPassword
$envContent = Set-DotEnvValue $envContent 'RABBITMQ_USERNAME' $rabbitUser
$envContent = Set-DotEnvValue $envContent 'RABBITMQ_PASSWORD' $rabbitPassword
$envContent = Set-DotEnvValue $envContent 'JWT_KEY' $jwtKey
[System.IO.File]::WriteAllText($envPath, $envContent, [System.Text.UTF8Encoding]::new($false))

Write-Output 'Local PostgreSQL, RabbitMQ, and JWT secrets were generated and stored in the ignored .env file.'
if (-not $RotateRunningServices) {
    Write-Warning 'Existing database/broker volumes were not updated. Use -RotateRunningServices while both services are running.'
}
