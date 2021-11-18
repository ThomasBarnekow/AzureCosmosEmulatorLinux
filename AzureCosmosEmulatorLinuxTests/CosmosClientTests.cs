using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Abstractions;

namespace AzureCosmosEmulatorLinuxTests
{
    /// <summary>
    ///     Tests connectivity to the Azure Cosmos Emulator.
    ///     The Docker container is configured without setting:
    ///     AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=$ipaddr
    ///     In this case:
    ///     - on Ubunto on WSL, the Gateweay mode works but the Direct mode does not work; and
    ///     - on appveyor, all connection modes work.
    ///     When configuring the Docker container as suggested by Microsoft, i.e., when setting the
    ///     above environment variable to the local IP address:
    ///     - on Ubuntu on WSL, all connection modes work; and
    ///     - on appveyor, NO connection mode works.
    /// </summary>
    public class CosmosClientTests
    {
        private const string AccountKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private const string DatabaseId = "TestDatabase";

        private const string ContainerId = "TestContainer";

        private const string PartitionKeyPath = "/" + nameof(TestItem.PartitionKey);

        private readonly ITestOutputHelper _output;

        private string? _connectionString;

        private IPAddress? _localIpAddress;

        public CosmosClientTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private IPAddress LocalIpAddress =>
            _localIpAddress ??= NetworkInterface.GetAllNetworkInterfaces()
                //.Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                //              nic.GetIPProperties().GatewayAddresses.Any())
                .Where(nic => nic.Name == "docker0")
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(ua => ua.Address)
                .First();

        private string ConnectionString =>
            _connectionString ??= $"AccountEndpoint=https://{LocalIpAddress}:8081/;AccountKey={AccountKey}";

        [Theory]
        [InlineData(ConnectionMode.Direct)]
        [InlineData(ConnectionMode.Gateway)]
        public async Task CanConnectToAzureCosmosEmulator(ConnectionMode connectionMode)
        {
            _output.WriteLine($"Testing with ConnectionMode = '{connectionMode}' ...");

            _output.WriteLine("Testing with 'localhost' ...");
            var success = await TryConnectToAzureCosmosEmulator(connectionMode, "localhost");

            var ipAddresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                              nic.GetIPProperties().GatewayAddresses.Any())
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(ua => ua.Address.ToString());

            foreach (var ipAddress in ipAddresses)
            {
                _output.WriteLine($"Testing with IP address = '{ipAddress}' ...");
                success &= await TryConnectToAzureCosmosEmulator(connectionMode, ipAddress);
            }

            if (!success)
            {
                Assert.True(false, "At least one test failed.");
            }
        }

        private async Task<bool> TryConnectToAzureCosmosEmulator(ConnectionMode connectionMode, string ipAddress)
        {
            try
            {
                CosmosClientOptions clientOptions = new()
                {
                    ConnectionMode = connectionMode,
                    LimitToEndpoint = true,
                    RequestTimeout = TimeSpan.FromSeconds(30)
                };

                CosmosClient client = new($"https://{ipAddress}:8081/", AccountKey, clientOptions);
                DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);

                _output.WriteLine($"{ipAddress} / {connectionMode} succeeded:");
                _output.WriteLine($"StatusCode = {response.StatusCode}.");

                return true;
            }
            catch (Exception e)
            {
                _output.WriteLine($"{ipAddress} / {connectionMode} FAILED!!!");
                _output.WriteLine($"Exception Message = '{e.Message}'");
                _output.WriteLine($"InnerException Message = '{e.InnerException?.Message}'");
                return false;
            }
        }

        //[Theory]
        //[InlineData(ConnectionMode.Direct)]
        //[InlineData(ConnectionMode.Gateway)]
        //public async Task CanCreateContainer(ConnectionMode connectionMode)
        //{
        //    _output.WriteLine($"Testing with ConnectionMode = '{connectionMode}' ...");

        //    // Arrange
        //    CosmosClientOptions clientOptions = new()
        //    {
        //        HttpClientFactory = () =>
        //        {
        //            HttpMessageHandler httpMessageHandler = new HttpClientHandler
        //            {
        //                ServerCertificateCustomValidationCallback =
        //                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        //            };

        //            return new HttpClient(httpMessageHandler);
        //        },
        //        ConnectionMode = connectionMode,
        //        LimitToEndpoint = true,
        //        RequestTimeout = TimeSpan.FromSeconds(5)
        //    };

        //    CosmosClient client = new(ConnectionString, clientOptions);

        //    Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);

        //    // Act
        //    ContainerResponse response = await database.CreateContainerIfNotExistsAsync(ContainerId, PartitionKeyPath);

        //    // Assert
        //    response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        //    _output.WriteLine($"Created container if it did not exist. StatusCode = {response.StatusCode}.");
        //}

        //[Theory]
        //[InlineData(ConnectionMode.Direct)]
        //[InlineData(ConnectionMode.Gateway)]
        //public async Task CanCreateItem(ConnectionMode connectionMode)
        //{
        //    _output.WriteLine($"Testing with ConnectionMode = '{connectionMode}' ...");

        //    // Arrange
        //    CosmosClientOptions clientOptions = new()
        //    {
        //        ConnectionMode = connectionMode,
        //        LimitToEndpoint = true,
        //        RequestTimeout = TimeSpan.FromSeconds(5)
        //    };

        //    CosmosClient client = new(ConnectionString, clientOptions);

        //    Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
        //    Container container = await database.CreateContainerIfNotExistsAsync(ContainerId, PartitionKeyPath);

        //    string itemId = Guid.NewGuid().ToString();

        //    TestItem item = new()
        //    {
        //        Id = itemId,
        //        Description = "Item created by integration test"
        //    };

        //    // Act
        //    ItemResponse<TestItem> response =
        //        await container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.Created);

        //    _output.WriteLine($"Created item. StatusCode = {response.StatusCode}.");
        //}
    }
}
