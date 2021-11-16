# Azure Cosmos Emulator for Linux

## Purpose

This repository contains an integration test project and build scripts that demonstrate how the Azure Cosmos Emulator
can be run on Linux and in particular on appveyor.

## Background

Running the Docker container on appveyor did not work with the configuration recommended in the documentation
(see [Run the emulator on Docker for Linux](https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator)).
Specifically, the official recommendation is to set the environment variable `AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE`
to the local IP address to enable Direct mode. While this works on my local machine, using Ubuntu on WSL, this does
not work at all on appveyor. When setting the above environment variable, NO connection mode works on appveyor,
meaning that all integration tests depending on the Azure Cosmos Emulator will fail.

## Solution

The Docker container should be run with the following command, which does not set the above environment variable:

```
docker run \
  -p 8081:8081 \
  -p 10251:10251 \
  -p 10252:10252 \
  -p 10253:10253 \
  -p 10254:10254 \
  -m 3g \
  --cpus=2.0 \
  --name=azure-cosmos-emulator-linux \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
  -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=false \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator
```

Lets look at the parameters:

- `-m 3g` sets 3 GB of memory, which is the minimum required. It was enough for simple integration tests.
- 2 CPU cores is the value recommended by Microsoft. You can reduce this down to 0.5 cores, but the documentation
  says this is going to be very slow.
- You can obviously pick your own name. "azure-cosmos-emulator-linux" is just my choice.
- You must pick a number of partitions that suits your needs. 3 partitions were enough for a simple integration
  test in which I only created a single container. In that case, 1 partition might have done the job. As you
  will see, the container will always create one additional partition.
- If you are only using the container for integration tests on appveyor, you can turn off data persistence.
- Again, the above command obviously does not override the IP address. This is the key to making it work on
  appveyor.

The `install-azure-cosmos-emulator-linux-certificates.sh` uses the following command to download the certificate:

```
curl -k https://localhost:8081/_explorer/emulator.pem > $certfile
```

This also works in case you override the IP address. In all cases, the certificate returned is for `CN=localhost`.
In other words, it is not necessary to determine the IP address as suggested in the documentationto download
the certificate. However, you must install the certificate as stated in the documentation to successfully connect
via SSL.
