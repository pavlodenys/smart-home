[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$envPath = Join-Path $repositoryRoot '.env'

if (-not (Test-Path -LiteralPath $envPath)) {
    throw 'The root .env file does not exist.'
}

$values = @{}
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') {
        $values[$matches[1]] = $matches[2]
    }
}

$expectedUsername = $values['RABBITMQ_USERNAME']
$expectedPassword = $values['RABBITMQ_PASSWORD']
if (-not $expectedUsername -or -not $expectedPassword) {
    throw 'RabbitMQ credentials are missing from .env.'
}

$configs = @(
    @{
        Path = 'sensors/dht11/include/config.h'
        UsernamePattern = '(?m)^\s*const char \*user\s*=\s*"([^"]*)"'
        PasswordPattern = '(?m)^\s*const char \*pass\s*=\s*"([^"]*)"'
    },
    @{
        Path = 'sensors/soil-moisture-v2/include/config.h'
        UsernamePattern = '(?m)^\s*constexpr char MQTT_USERNAME\[\]\s*=\s*"([^"]*)"'
        PasswordPattern = '(?m)^\s*constexpr char MQTT_PASSWORD\[\]\s*=\s*"([^"]*)"'
    }
)

$verifiedCount = 0
foreach ($config in $configs) {
    $path = Join-Path $repositoryRoot $config.Path
    if (-not (Test-Path -LiteralPath $path)) {
        continue
    }

    $content = [System.IO.File]::ReadAllText($path)
    $usernameMatch = [regex]::Match($content, $config.UsernamePattern)
    $passwordMatch = [regex]::Match($content, $config.PasswordPattern)
    if (-not $usernameMatch.Success -or -not $passwordMatch.Success) {
        throw "Credential declarations are missing from $($config.Path)."
    }
    if ($usernameMatch.Groups[1].Value -cne $expectedUsername -or
        $passwordMatch.Groups[1].Value -cne $expectedPassword) {
        throw "MQTT credentials are stale in $($config.Path)."
    }

    git -C $repositoryRoot check-ignore --quiet -- $config.Path
    if ($LASTEXITCODE -ne 0) {
        throw "$($config.Path) contains credentials but is not ignored by Git."
    }
    $verifiedCount++
}

if ($verifiedCount -eq 0) {
    throw 'No local firmware configuration files were found.'
}

Write-Output "Verified MQTT credential synchronization for $verifiedCount ignored firmware configuration file(s)."
