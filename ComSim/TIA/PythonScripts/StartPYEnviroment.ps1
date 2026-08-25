Write-Host "Check virtual enviroment..."

if (Test-Path ".\venv\pyvenv.cfg") {
    $configuredPython = (Select-String -Path ".\venv\pyvenv.cfg" -Pattern '^executable\s*=\s*(.+)$').Matches.Groups[1].Value.Trim()
    if ($configuredPython -and -Not (Test-Path -LiteralPath $configuredPython)) {
        Write-Host "Existing virtual environment belongs to another computer. Recreating it..."
        Remove-Item -LiteralPath ".\venv" -Recurse -Force
    }
}

if (-Not (Test-Path ".\venv")) {
    Write-Host "Venv not found, creating new..."
    python -m venv venv

    Write-Host "Activating new virtual enviroment..."
    & .\venv\Scripts\Activate.ps1

    if (Test-Path ".\requirements.txt") {
        Write-Host "Installing libraris from requirements.txt..."
        pip install --upgrade pip
        pip install -r .\requirements.txt
    }
}
else {
    Write-Host "Venv exists..."
    & .\venv\Scripts\Activate.ps1
    Write-Host "Virtual enviroment activated...."
}

#Write-Host "Calling python script..."

#python code.py

#deactivate



