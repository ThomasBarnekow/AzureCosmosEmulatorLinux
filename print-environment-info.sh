#!/bin/bash

echo "## BUILD AND TEST ENVIRONMENT"

echo
echo "### DOTNET"
echo
echo The dotnet version is `dotnet --version`.

echo
echo "### DOCKER CONTAINERS"

echo
echo 'This is the list of containers (`docker ps --all`):'
echo
echo '```'
docker ps --all
echo '```'

echo
echo 'Here are the port mappings (`docker port azure-cosmos-emulator-linux`):'
echo
echo '```'
docker port azure-cosmos-emulator-linux
echo '```'

echo
ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
echo "The Docker container was run with AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=$ipaddr."
echo

echo
echo "### NETWORK INTERFACE CONFIGURATION"
echo
echo '```'
ifconfig
echo '```'

echo
echo "### LISTENING TCP SOCKETS"
echo
echo '```'
netstat -lt
echo '```'

echo
echo "### EMULATOR CERTIFICATE"

echo
echo Using '`curl -k https://localhost:8081/_explorer/emulator.pem`:'
echo
echo '```'
curl -k https://localhost:8081/_explorer/emulator.pem
echo '```'

echo
echo Using '`Invoke-WebRequest -Uri https://localhost:8081/_explorer/emulator.pem`:'
echo
echo '```'
pwsh ./GetEmulatorCertificate.ps1
echo '```'
