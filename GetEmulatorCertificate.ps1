param ($address)

Write-Host "Getting certificate from: https://${address}:8081/_explorer/emulator.pem"
Invoke-WebRequest -Uri "https://${address}:8081/_explorer/emulator.pem"

Write-Host "Getting certificate from: https://localhost:8081/_explorer/emulator.pem"
Invoke-WebRequest -Uri "https://localhost:8081/_explorer/emulator.pem"
