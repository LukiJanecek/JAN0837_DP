<#
.SYNOPSIS
    Připraví projekt JAN0837_DP ke spuštění na cílovém počítači.

.DESCRIPTION
    Skript pracuje PŘÍMO v aktuální složce projektu. Projekt nikam nekopíruje.

    Postup:
      1. Vyžádá oprávnění správce.
      2. Zkontroluje/doinstaluje .NET 8 SDK, Node.js LTS, Python a WebView2.
      3. Najde nejnovější TIA Portal PublicAPI (Siemens.Engineering.dll).
      4. Přidá uživatele do skupiny "Siemens TIA Openness".
      5. Nastaví nalezenou cestu TIA v csproj a internalVariables.cs.
      6. Spustí npm ci pro React.
      7. Vytvoří Python venv a nainstaluje pythonnet.
      8. Publikuje x64 aplikaci do bin\Release\net8.0-windows.
      9. Volitelně vytvoří zástupce na ploše parametrem -CreateShortcut.

    TIA Portal a jeho licenci skript neinstaluje. TIA Portal musí být na PC předem.
    Po přidání uživatele do skupiny TIA Openness je nutné odhlášení nebo restart.

.EXAMPLE
    powershell.exe -ExecutionPolicy Bypass -File .\deploy.ps1

.EXAMPLE
    powershell.exe -ExecutionPolicy Bypass -File .\deploy.ps1 -CreateShortcut
#>

[CmdletBinding()]
param(
    [switch]$SkipPrerequisites,
    [switch]$SkipFirewall,
    [switch]$CreateShortcut
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$ProjectRoot = $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "JAN0837_DP.csproj"
$VariablesFile = Join-Path $ProjectRoot "Data\internalVariables.cs"
$FrontendDirectory = Join-Path $ProjectRoot "ReactFE\jan0837_reactfe"
$PythonDirectory = Join-Path $ProjectRoot "TIA\PythonScripts"
$PublishDirectory = Join-Path $ProjectRoot "bin\Release\net8.0-windows"
$OpennessGroup = "Siemens TIA Openness"
$FrontendPort = 3000
$ApiPort = 3001

function Write-Step([string]$Text) {
    Write-Host ""
    Write-Host "==> $Text" -ForegroundColor Cyan
}

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Restart-AsAdministrator {
    Write-Step "Vyžádání oprávnění správce"

    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", ('"{0}"' -f $PSCommandPath)
    )
    if ($SkipPrerequisites) { $arguments += "-SkipPrerequisites" }
    if ($SkipFirewall) { $arguments += "-SkipFirewall" }
    if ($CreateShortcut) { $arguments += "-CreateShortcut" }

    Start-Process "powershell.exe" -Verb RunAs -ArgumentList $arguments
    exit
}

function Refresh-Path {
    $machinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $env:Path = "$machinePath;$userPath"
}

function Test-Command([string]$Name) {
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Install-WingetPackage([string]$Id, [string]$Name) {
    if (-not (Test-Command "winget.exe")) {
        throw "Chybí winget. Nainstalujte Microsoft App Installer a spusťte deploy znovu."
    }

    Write-Host "Instaluji $Name..."
    & winget.exe install --id $Id --exact --silent `
        --accept-package-agreements --accept-source-agreements

    if ($LASTEXITCODE -ne 0) {
        throw "Instalace '$Name' selhala (exit code $LASTEXITCODE)."
    }
    Refresh-Path
}

function Get-PythonLauncher {
    if (Test-Command "py.exe") {
        $previousErrorPreference = $ErrorActionPreference
        try {
            $ErrorActionPreference = "Continue"
            & py.exe -3.13 --version *> $null
            $pythonExitCode = $LASTEXITCODE
        }
        catch {
            $pythonExitCode = 1
        }
        finally {
            $ErrorActionPreference = $previousErrorPreference
        }
        if ($pythonExitCode -eq 0) { return "py.exe" }
    }
    if (Test-Command "python.exe") {
        $previousErrorPreference = $ErrorActionPreference
        try {
            $ErrorActionPreference = "Continue"
            $pythonVersionOutput = & python.exe --version 2>&1
            $pythonExitCode = $LASTEXITCODE
            if ($pythonExitCode -eq 0 -and $pythonVersionOutput -notmatch '^Python 3\.13\.') {
                $pythonExitCode = 1
            }
        }
        catch {
            $pythonExitCode = 1
        }
        finally {
            $ErrorActionPreference = $previousErrorPreference
        }
        if ($pythonExitCode -eq 0) { return "python.exe" }
    }
    return $null
}

function Install-Prerequisites {
    Write-Step "Kontrola návazností"

    $hasDotnet8 = $false
    if (Test-Command "dotnet.exe") {
        $hasDotnet8 = $null -ne (& dotnet.exe --list-sdks |
            Where-Object { $_ -match "^8\." } |
            Select-Object -First 1)
    }
    if (-not $hasDotnet8) {
        Install-WingetPackage "Microsoft.DotNet.SDK.8" ".NET 8 SDK"
    }

    if (-not (Test-Command "node.exe") -or -not (Test-Command "npm.cmd")) {
        Install-WingetPackage "OpenJS.NodeJS.LTS" "Node.js LTS"
    }

    if ($null -eq (Get-PythonLauncher)) {
        Install-WingetPackage "Python.Python.3.13" "Python 3.13"
    }

    # WebView2 bývá ve Windows již nainstalovaný. Winget existující instalaci rozpozná.
    & winget.exe list --id "Microsoft.EdgeWebView2Runtime" --exact *> $null
    if ($LASTEXITCODE -ne 0) {
        Install-WingetPackage "Microsoft.EdgeWebView2Runtime" "Microsoft Edge WebView2 Runtime"
    }

    Refresh-Path
}

function Find-TiaPublicApi {
    Write-Step "Hledání Siemens.Engineering.dll"

    $roots = @(
        (Join-Path $env:ProgramFiles "Siemens\Automation"),
        (Join-Path ${env:ProgramFiles(x86)} "Siemens\Automation")
    ) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }

    $candidates = foreach ($root in $roots) {
        Get-ChildItem -LiteralPath $root -Recurse -Filter "Siemens.Engineering.dll" `
            -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\PublicAPI\\V[0-9.]+\\" } |
            ForEach-Object {
                $match = [regex]::Match($_.FullName, "\\PublicAPI\\V(?<version>[0-9.]+)\\")
                $version = [version]"0.0"
                if ($match.Success) {
                    [version]::TryParse($match.Groups["version"].Value, [ref]$version) | Out-Null
                }
                [PSCustomObject]@{
                    Version = $version
                    Dll = $_.FullName
                    Directory = $_.DirectoryName
                }
            }
    }

    $tia = $candidates | Sort-Object Version -Descending | Select-Object -First 1
    if ($null -eq $tia) {
        throw @"
Siemens.Engineering.dll nebyla nalezena.
TIA Portal musí být nainstalovaný včetně komponenty Openness/PublicAPI.
"@
    }

    Write-Host "Nalezeno: $($tia.Dll)" -ForegroundColor Green
    return $tia
}

function Add-UserToOpennessGroup {
    Write-Step "Kontrola skupiny Siemens TIA Openness"

    $group = Get-LocalGroup -Name $OpennessGroup -ErrorAction SilentlyContinue
    if ($null -eq $group) {
        throw "Skupina '$OpennessGroup' neexistuje. Zkontrolujte instalaci TIA Openness."
    }

    $user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $member = Get-LocalGroupMember -Group $OpennessGroup -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq $user }

    if ($member) {
        Write-Host "$user už je členem skupiny." -ForegroundColor Green
        return $false
    }

    Add-LocalGroupMember -Group $OpennessGroup -Member $user
    Write-Host "$user byl přidán do skupiny." -ForegroundColor Green
    return $true
}

function Set-TiaPath($Tia) {
    Write-Step "Nastavení cesty k TIA API v projektu"

    [xml]$projectXml = Get-Content -Raw -LiteralPath $ProjectFile
    $reference = $projectXml.Project.ItemGroup.Reference |
        Where-Object { $_.Include -eq "Siemens.Engineering" } |
        Select-Object -First 1

    if ($null -eq $reference) {
        throw "V JAN0837_DP.csproj chybí reference Siemens.Engineering."
    }

    $reference.HintPath = $Tia.Dll
    $projectXml.Save($ProjectFile)

    # Runtime cesta je zatím v aplikaci zadaná přímo v internalVariables.cs.
    # Dokud nebude přesunuta do konfiguračního souboru, upravíme ji při deploymentu.
    $csharpPath = $Tia.Directory.Replace("\", "\\")
    $content = Get-Content -Raw -LiteralPath $VariablesFile
    $content = [regex]::Replace(
        $content,
        '(public static string tiaDLLPath \{ get; set; \} = )"[^"]*"',
        ('$1"{0}"' -f $csharpPath)
    )
    $content = [regex]::Replace(
        $content,
        '(public static string defaultTIADLLPath \{ get; set; \} = )"[^"]*"',
        ('$1"{0}"' -f $csharpPath)
    )
    Set-Content -LiteralPath $VariablesFile -Value $content -Encoding UTF8
}

function Install-ProjectDependencies {
    Write-Step "Instalace React závislostí"
    Push-Location $FrontendDirectory
    try {
        & npm.cmd ci --no-audit --no-fund
        if ($LASTEXITCODE -ne 0) { throw "npm ci selhalo." }
    }
    finally {
        Pop-Location
    }

    Write-Step "Příprava Python virtual environment"
    $venv = Join-Path $PythonDirectory "venv"
    $venvPython = Join-Path $venv "Scripts\python.exe"

    if (-not (Test-Path -LiteralPath $venvPython)) {
        $launcher = Get-PythonLauncher
        if ($launcher -eq "py.exe") {
            & py.exe -3 -m venv $venv
        }
        elseif ($launcher -eq "python.exe") {
            & python.exe -m venv $venv
        }
        else {
            throw "Python nebyl nalezen."
        }
        if ($LASTEXITCODE -ne 0) { throw "Vytvoření Python venv selhalo." }
    }

    # TODO: requirements.txt obsahuje také os/sys/pathlib, které jsou součástí Pythonu
    # a nelze je instalovat přes pip. Proto zatím instalujeme jen externí pythonnet.
    & $venvPython -m pip install --disable-pip-version-check pythonnet
    if ($LASTEXITCODE -ne 0) { throw "Instalace pythonnet selhala." }
}

function Publish-Project {
    Write-Step "Publikování aplikace"

    & dotnet.exe restore $ProjectFile --runtime win-x64
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore selhal." }

    # Tato cesta zachovává současné očekávání aplikace, že kořen projektu leží
    # tři úrovně nad Application.StartupPath.
    & dotnet.exe publish $ProjectFile `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --no-restore `
        --output $PublishDirectory `
        -p:PublishSingleFile=false `
        -p:DebugType=None `
        -p:DebugSymbols=false

    if ($LASTEXITCODE -ne 0) { throw "dotnet publish selhal." }

    $exe = Join-Path $PublishDirectory "ComSim.exe"
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Výsledné EXE nebylo nalezeno: $exe"
    }
    return $exe
}

function Configure-Network([bool]$ConfigureFirewall = $true) {
    Write-Step "Kontrola portů a nastavení HTTP URL ACL/firewallu"

    $user = [Security.Principal.WindowsIdentity]::GetCurrent().Name

    # Rezervujeme kořen portu. Tím jsou pokryté jak /api/, tak OWIN statický server.
    # Předchozí verze rezervovala pouze /api/, což na čisté instalaci Windows 11 nestačilo.
    $apiUrl = "http://+:$ApiPort/api/"
    foreach ($url in @($apiUrl)) {
        & netsh.exe http delete urlacl url="$url" *> $null
    }
    & netsh.exe http add urlacl url="$apiUrl" user="$user" listen=yes delegate=no
    if ($LASTEXITCODE -ne 0) {
        throw "Nepodařilo se vytvořit HTTP URL ACL pro uživatele $user."
    }

    & netsh.exe http show urlacl url="$apiUrl" *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "HTTP URL ACL $apiUrl se po vytvoreni nepodarilo overit."
    }
    Write-Host "HTTP URL ACL: $apiUrl ($user)" -ForegroundColor Green

    foreach ($port in @($FrontendPort, $ApiPort)) {
        $listeners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
        if ($listeners) {
            $processes = $listeners |
                ForEach-Object { Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue } |
                Select-Object -ExpandProperty ProcessName -Unique
            Write-Host "VAROVÁNÍ: Port $port už používá: $($processes -join ', ')" `
                -ForegroundColor Yellow
        }

        if (-not $ConfigureFirewall) {
            continue
        }

        $name = "JAN0837_DP - TCP $port"
        Remove-NetFirewallRule -DisplayName $name -ErrorAction SilentlyContinue
        New-NetFirewallRule -DisplayName $name -Direction Inbound -Action Allow `
            -Protocol TCP -LocalPort $port -Profile Any | Out-Null
    }

    if (-not $ConfigureFirewall) {
        Write-Host "Firewall byl preskocen; povinna HTTP URL ACL byla vytvorena." `
            -ForegroundColor Yellow
    }
}

function New-ApplicationShortcut([string]$Executable) {
    Write-Step "Vytvoření zástupce na ploše"

    $desktop = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = Join-Path $desktop "JAN0837_DP.lnk"
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $Executable
    $shortcut.WorkingDirectory = Split-Path -Parent $Executable
    $shortcut.Description = "JAN0837_DP"
    $shortcut.IconLocation = "$Executable,0"
    $shortcut.Save()
}

# Compatible deployment path for both the legacy monolithic API and V21+ modular API.
function Find-TiaPublicApiCompatible {
    Write-Step "Hledani TIA Portal Openness API (legacy nebo V21+)"

    $roots = @(
        (Join-Path $env:ProgramFiles "Siemens\Automation"),
        (Join-Path ${env:ProgramFiles(x86)} "Siemens\Automation")
    ) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }

    $apiFiles = foreach ($root in $roots) {
        Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Name -in @("Siemens.Engineering.dll", "Siemens.Engineering.Base.dll") -and
                $_.FullName -match "\\PublicAPI\\V[0-9.]+\\"
            }
    }

    $candidates = $apiFiles | ForEach-Object {
        $match = [regex]::Match($_.FullName, "\\PublicAPI\\V(?<version>[0-9.]+)\\")
        $version = [version]"0.0"
        if ($match.Success) {
            [version]::TryParse($match.Groups["version"].Value, [ref]$version) | Out-Null
        }
        [PSCustomObject]@{
            Version = $version
            Dll = $_.FullName
            Directory = $_.DirectoryName
            ApiKind = if ($_.Name -eq "Siemens.Engineering.Base.dll") { "V21+ modular" } else { "legacy" }
        }
    }

    $tia = $candidates | Sort-Object Version -Descending | Select-Object -First 1
    if ($null -eq $tia) {
        throw "TIA Portal Openness API nebylo nalezeno. Nainstalujte komponentu Openness/PublicAPI."
    }
    Write-Host "Nalezeno: $($tia.Dll) [$($tia.ApiKind)]" -ForegroundColor Green
    return $tia
}

function Confirm-TiaPath($Tia) {
    Write-Step "Overeni cesty k TIA API"
    if (-not (Test-Path -LiteralPath $Tia.Dll -PathType Leaf)) {
        throw "Nalezene TIA API neni dostupne: $($Tia.Dll)"
    }
    Write-Host "TIA API se bude nacitat dynamicky z: $($Tia.Directory)" -ForegroundColor Green
}

function Install-ProjectDependenciesCompatible {
    Write-Step "Instalace React zavislosti"
    Push-Location $FrontendDirectory
    try {
        & npm.cmd ci --no-audit --no-fund
        if ($LASTEXITCODE -ne 0) { throw "npm ci selhalo." }
    }
    finally {
        Pop-Location
    }

    Write-Step "Priprava lokalniho Python virtual environment"
    $venv = Join-Path $PythonDirectory "venv"
    $venvPython = Join-Path $venv "Scripts\python.exe"
    $venvConfig = Join-Path $venv "pyvenv.cfg"
    $recreateVenv = -not (Test-Path -LiteralPath $venvPython)

    if (-not $recreateVenv -and (Test-Path -LiteralPath $venvConfig)) {
        $executableLine = Get-Content -LiteralPath $venvConfig |
            Where-Object { $_ -match '^executable\s*=\s*(.+)$' } |
            Select-Object -First 1
        if ($executableLine -match '^executable\s*=\s*(.+)$') {
            $basePython = $Matches[1].Trim()
            if (-not (Test-Path -LiteralPath $basePython -PathType Leaf)) {
                Write-Host "Preneseny venv odkazuje na $basePython a bude vytvoren znovu." -ForegroundColor Yellow
                $recreateVenv = $true
            }
        }
    }

    if ($recreateVenv -and (Test-Path -LiteralPath $venv)) {
        $resolvedVenv = (Resolve-Path -LiteralPath $venv).Path
        $resolvedPythonDirectory = (Resolve-Path -LiteralPath $PythonDirectory).Path
        if (-not $resolvedVenv.StartsWith($resolvedPythonDirectory, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Odmitnuto odstraneni neocekavaneho venv: $resolvedVenv"
        }
        Remove-Item -LiteralPath $resolvedVenv -Recurse -Force
    }

    if ($recreateVenv) {
        $launcher = Get-PythonLauncher
        if ($launcher -eq "py.exe") {
            & py.exe -3 -m venv $venv
        }
        elseif ($launcher -eq "python.exe") {
            & python.exe -m venv $venv
        }
        else {
            throw "Python nebyl nalezen ani po instalaci prerequisites."
        }
        if ($LASTEXITCODE -ne 0) { throw "Vytvoreni Python venv selhalo." }
    }

    & $venvPython --version
    if ($LASTEXITCODE -ne 0) { throw "Lokalni Python venv nelze spustit." }
    $requirements = Join-Path $PythonDirectory "requirements.txt"
    & $venvPython -m pip install --disable-pip-version-check --upgrade pip
    if ($LASTEXITCODE -ne 0) { throw "Aktualizace pip selhala." }
    & $venvPython -m pip install --disable-pip-version-check -r $requirements
    if ($LASTEXITCODE -ne 0) { throw "Instalace Python zavislosti selhala." }
    & $venvPython -c "import clr; print('pythonnet OK')"
    if ($LASTEXITCODE -ne 0) { throw "Overeni pythonnet selhalo." }
}

if (-not (Test-Administrator)) {
    Restart-AsAdministrator
}

if (-not (Test-Path -LiteralPath $ProjectFile) -or
    -not (Test-Path -LiteralPath (Join-Path $ProjectRoot "Program.cs"))) {
    throw "deploy.ps1 musí být umístěný vedle Program.cs a JAN0837_DP.csproj."
}

$LogDirectory = Join-Path $ProjectRoot "Log"
New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
$LogFile = Join-Path $LogDirectory ("deploy-{0:yyyyMMdd-HHmmss}.log" -f (Get-Date))
Start-Transcript -Path $LogFile -Force | Out-Null

try {
    Write-Host "JAN0837_DP deployment" -ForegroundColor White
    Write-Host "Projekt zůstává v: $ProjectRoot"

    if (-not $SkipPrerequisites) {
        Install-Prerequisites
    }
    else {
        Refresh-Path
        Write-Host "Instalace návazností přeskočena." -ForegroundColor Yellow
    }

    $tia = Find-TiaPublicApiCompatible
    $restartRequired = Add-UserToOpennessGroup
    Confirm-TiaPath $tia
    Install-ProjectDependenciesCompatible
    $exe = Publish-Project

    Configure-Network -ConfigureFirewall:(-not $SkipFirewall)

    if ($CreateShortcut) {
        New-ApplicationShortcut $exe
    }

    Write-Host ""
    Write-Host "DEPLOYMENT DOKONČEN" -ForegroundColor Green
    Write-Host "EXE: $exe"
    Write-Host "Log: $LogFile"

    if ($restartRequired) {
        Write-Host "Odhlaste se nebo restartujte PC kvůli skupině TIA Openness." `
            -ForegroundColor Yellow
    }
}
finally {
    Stop-Transcript | Out-Null
}
