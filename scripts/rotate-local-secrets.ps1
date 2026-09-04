[CmdletBinding()]
param(
    [switch]$RotateRunningServices,
    [switch]$SyncFirmwareOnly
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$envPath = Join-Path $repositoryRoot '.env'

function New-RandomSecret([int]$ByteCount = 48) {
    $bytes = [byte[]]::new($ByteCount)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
    } finally {
        $rng.Dispose()
    }
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

function Get-DotEnvValues([string]$Path) {
    $values = @{}
    Get-Content -LiteralPath $Path | ForEach-Object {
        if ($_ -match '^([^#=]+)=(.*)$') {
            $values[$matches[1]] = $matches[2]
        }
    }
    return $values
}

function Set-CppString([string]$Content, [string]$Pattern, [string]$Value) {
    $match = [regex]::Match($Content, $Pattern)
    if (-not $match.Success) {
        throw "Expected firmware configuration field was not found."
    }

    return [regex]::Replace(
        $Content,
        $Pattern,
        { param($item) $item.Groups[1].Value + '"' + $Value + '"' },
        1)
}

function Sync-FirmwareMqttCredentials([string]$Username, [string]$Password) {
    $updatedCount = 0
    $configs = @(
        @{
            Path = 'sensors/dht11/include/config.h'
            UsernamePattern = '(?m)^(\s*const char \*user\s*=\s*)"[^"]*"'
            PasswordPattern = '(?m)^(\s*const char \*pass\s*=\s*)"[^"]*"'
        },
        @{
            Path = 'sensors/soil-moisture-v2/include/config.h'
            UsernamePattern = '(?m)^(\s*constexpr char MQTT_USERNAME\[\]\s*=\s*)"[^"]*"'
            PasswordPattern = '(?m)^(\s*constexpr char MQTT_PASSWORD\[\]\s*=\s*)"[^"]*"'
        }
    )

    foreach ($config in $configs) {
        $path = Join-Path $repositoryRoot $config.Path
        if (-not (Test-Path -LiteralPath $path)) {
            continue
        }

        $content = [System.IO.File]::ReadAllText($path)
        $content = Set-CppString $content $config.UsernamePattern $Username
        $content = Set-CppString $content $config.PasswordPattern $Password
        [System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($false))
        $updatedCount++
    }

    Write-Output "Synchronized MQTT credentials in $updatedCount ignored firmware configuration file(s)."
}

if ($SyncFirmwareOnly) {
    if (-not (Test-Path -LiteralPath $envPath)) {
        throw 'The root .env file is required for firmware synchronization.'
    }

    $currentValues = Get-DotEnvValues $envPath
    if (-not $currentValues['RABBITMQ_USERNAME'] -or -not $currentValues['RABBITMQ_PASSWORD']) {
        throw 'RABBITMQ_USERNAME and RABBITMQ_PASSWORD are required in .env.'
    }

    Sync-FirmwareMqttCredentials `
        $currentValues['RABBITMQ_USERNAME'] `
        $currentValues['RABBITMQ_PASSWORD']
    return
}

$postgresPassword = New-RandomSecret
$rabbitPassword = New-RandomSecret
$jwtKey = New-RandomSecret 64
$ingestApiKey = New-RandomSecret
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
$envContent = Set-DotEnvValue $envContent 'INGEST_API_KEY' $ingestApiKey
[System.IO.File]::WriteAllText($envPath, $envContent, [System.Text.UTF8Encoding]::new($false))
Sync-FirmwareMqttCredentials $rabbitUser $rabbitPassword

Write-Output 'Local PostgreSQL, RabbitMQ, and JWT secrets were generated and stored in the ignored .env file.'
if (-not $RotateRunningServices) {
    Write-Warning 'Existing database/broker volumes were not updated. Use -RotateRunningServices while both services are running.'
}
