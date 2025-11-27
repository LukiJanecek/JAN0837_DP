Write-Host "Check virtual enviroment..."

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



