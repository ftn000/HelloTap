# HelloTap WebGL Build & Deploy Script
param(
    [string]$ServerHost = "109.69.17.170",
    [string]$ServerUser = "root",
    [string]$RemoteDir = "/opt/hellotap/webgl",
    [string]$UnityPath = "E:\unity\6000.3.11f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
$ProjectPath = $PSScriptRoot
$BuildOutputDir = Join-Path $ProjectPath "Builds\WebGL"
$LogFile = Join-Path $ProjectPath "build_webgl.log"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  HelloTap WebGL Build & Deploy Pipeline " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. Build WebGL
Write-Host "`n[1/3] Building WebGL via Unity batchmode..." -ForegroundColor Yellow
if (Test-Path $LogFile) { Remove-Item $LogFile -Force }

$unityArgs = @(
    "-quit",
    "-batchmode",
    "-nographics",
    "-projectPath", "`"$ProjectPath`"",
    "-executeMethod", "HelloTap.Editor.WebGLBuilder.BuildWebGL",
    "-logFile", "`"$LogFile`""
)

$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow

if ($process.ExitCode -ne 0 -or !(Test-Path (Join-Path $BuildOutputDir "index.html"))) {
    Write-Host "[ERROR] Unity WebGL build failed with exit code $($process.ExitCode)!" -ForegroundColor Red
    if (Test-Path $LogFile) {
        Get-Content $LogFile -Tail 40 | Write-Host -ForegroundColor DarkRed
    }
    exit 1
}

Write-Host "[SUCCESS] WebGL build created at $BuildOutputDir" -ForegroundColor Green

# 2. Upload to Server
Write-Host "`n[2/3] Uploading WebGL build to $ServerUser@$ServerHost:$RemoteDir..." -ForegroundColor Yellow
ssh -o StrictHostKeyChecking=no "$ServerUser@$ServerHost" "mkdir -p $RemoteDir"

# Use scp to sync build files
scp -r -o StrictHostKeyChecking=no "$BuildOutputDir\*" "$ServerUser@${ServerHost}:$RemoteDir/"
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to upload files via SCP!" -ForegroundColor Red
    exit 1
}
Write-Host "[SUCCESS] Files uploaded successfully." -ForegroundColor Green

# 3. Reload Nginx & Test
Write-Host "`n[3/3] Verifying server response..." -ForegroundColor Yellow
ssh -o StrictHostKeyChecking=no "$ServerUser@$ServerHost" "nginx -t && systemctl reload nginx"

$url = "http://$ServerHost/hellotap/"
Write-Host "`n[DONE] WebGL deployed and ready to play!" -ForegroundColor Green
Write-Host "URL: $url" -ForegroundColor Cyan
