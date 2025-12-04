# build-and-package.ps1
# Run from a Windows developer PowerShell with Visual Studio and Inno Setup installed.

$solutionDir = (Resolve-Path "..").Path
$projectDir = Join-Path $solutionDir "KSHub"
$outDir = Join-Path $projectDir "bin\Release\net6.0-windows"
$innoScript = Join-Path $solutionDir "installer\kshub_installer.iss"

# 1) restore & build
dotnet restore $projectDir
dotnet publish $projectDir -c Release -r win-x64 /p:PublishSingleFile=false -o $outDir

if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

# 2) Copy CefSharp native dependencies (you may need to customize based on CefSharp package)
# CefSharp provides a Utility to copy runtimes; for simplicity, we expect NuGet to have added them to output.
# If not, follow CefSharp docs to copy x86/x64 native DLLs into $outDir

# 3) Prepare installer input folder
$installerInput = Join-Path $solutionDir "installer_input"
Remove-Item $installerInput -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $installerInput | Out-Null
Copy-Item -Path "$outDir\*" -Destination $installerInput -Recurse -Force

# Include sample-extension and assets
Copy-Item -Path (Join-Path $solutionDir "sample-extension") -Destination (Join-Path $installerInput "sample-extension") -Recurse -Force
Copy-Item -Path (Join-Path $solutionDir "assets") -Destination (Join-Path $installerInput "assets") -Recurse -Force

# 4) Run Inno Setup to build installer (ISCC must be in PATH)
$iscc = "ISCC.exe"
if (-not (Get-Command $iscc -ErrorAction SilentlyContinue)) { Write-Error "ISCC.exe not found. Install Inno Setup and add to PATH."; exit 1 }

# Create compiled installer output directory
$setupOutDir = Join-Path $solutionDir "installer_output"
New-Item -ItemType Directory -Path $setupOutDir -Force | Out-Null

# Prepare a temporary ISS by replacing placeholder paths in the template
$issTemplate = Get-Content $innoScript -Raw
$issFilled = $issTemplate -replace "{{InstallerInput}}", ($installerInput -replace "\\","\\")
$issFilled = $issFilled -replace "{{OutputDir}}", ($setupOutDir -replace "\\","\\")
$temporaryIss = Join-Path $env:TEMP "kshub_temp.iss"
$issFilled | Out-File -FilePath $temporaryIss -Encoding utf8

# Run ISCC
& $iscc $temporaryIss
if ($LASTEXITCODE -ne 0) { Write-Error "Inno Setup build failed"; exit 1 }

Write-Host "Installer created in $setupOutDir"
