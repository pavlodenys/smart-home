[CmdletBinding()]
param(
    [string]$VhdxPath = "D:\Users\Pavlo\AppData\Local\Docker\DockerDesktopWSL\DockerDesktopWSL\disk\docker_data.vhdx"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $VhdxPath)) {
    throw "Docker vhdx not found at $VhdxPath. Run 'docker info' or check CustomWslDistroDir in %APPDATA%\Docker\settings-store.json."
}

function Get-SizeGB([string]$Path) {
    return [math]::Round((Get-Item -LiteralPath $Path).Length / 1GB, 2)
}

$beforeGB = Get-SizeGB $VhdxPath
Write-Output "Vhdx size before compact: $beforeGB GB"

Write-Output 'Shutting down WSL (stops the Docker Desktop VM)...'
wsl --shutdown

$diskpartScript = Join-Path $env:TEMP "compact-docker-vhdx-$([guid]::NewGuid()).txt"
try {
    @"
select vdisk file="$VhdxPath"
attach vdisk readonly
compact vdisk
detach vdisk
"@ | Set-Content -Path $diskpartScript -Encoding ASCII

    diskpart /s $diskpartScript | Out-Null
} finally {
    Remove-Item -LiteralPath $diskpartScript -Force -ErrorAction SilentlyContinue
}

$afterGB = Get-SizeGB $VhdxPath
Write-Output "Vhdx size after compact: $afterGB GB"
Write-Output "Reclaimed approximately $([math]::Round($beforeGB - $afterGB, 2)) GB on disk."
Write-Output 'Start Docker Desktop again to resume normal use.'
