#!/bin/bash

certfile=~/emulatorcert.crt
echo "Certificate file: ${certfile}"

ipaddr="`ifconfig docker0 | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
echo "Downloading SSL certificate from IP address $ipaddr"

result=1
count=0
sleepTime=10

while [[ "$result" != "0" && "$count" < "5" ]]; do
  echo "Trying to download certificate ..."
  curl -k https://$ipaddr:8081/_explorer/emulator.pem
  result=$?
  let "count++"

  if [[ "$result" != "0" && "$count" < "5"  ]]
  then
    echo "Could not download certificate. Waiting $sleepTime seconds before trying again ..."
    sleep $sleepTime
    let sleepTime=sleepTime*2
  fi
done

if [[ $result -eq 0  ]]
then
  # To be on the safe side, download certificate again
  curl -k https://$ipaddr:8081/_explorer/emulator.pem > $certfile

  echo "Downloaded certificate:"
  cat $certfile
  echo

  echo "Certificate text:"
  openssl x509 -in $certfile -text
  echo

  echo "Updating CA certificates ..."
  sudo cp $certfile /usr/local/share/ca-certificates
  sudo update-ca-certificates
  echo

else
  echo "Could not download CA certificate!"
  false
fi
