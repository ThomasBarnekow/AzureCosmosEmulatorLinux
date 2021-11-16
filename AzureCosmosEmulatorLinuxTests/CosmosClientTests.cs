using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Xunit;
using Xunit.Abstractions;

namespace AzureCosmosEmulatorLinuxTests
{
    public class CosmosClientTests
    {
        private const string ConnectionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private const string DatabaseId = "TestDatabase";

        private const string ContainerId = "TestContainer";

        private const string PartitionKeyPath = "/" + nameof(TestItem.PartitionKey);

        private readonly ITestOutputHelper _output;

        public CosmosClientTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(ConnectionMode.Direct)]
        [InlineData(ConnectionMode.Gateway)]
        public async Task CanConnectToAzureCosmosEmulator(ConnectionMode connectionMode)
        {
            _output.WriteLine($"Testing with ConnectionMode = '{connectionMode}' ...");

            // Arrange
            CosmosClientOptions clientOptions = new()
            {
                ConnectionMode = connectionMode,
                LimitToEndpoint = true,
                RequestTimeout = TimeSpan.FromSeconds(5)
            };

            CosmosClient client = new(ConnectionString, clientOptions);

            // Act
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

            _output.WriteLine($"Created database if it did not exist. StatusCode = {response.StatusCode}.");
        }

        [Theory]
        [InlineData(ConnectionMode.Direct)]
        [InlineData(ConnectionMode.Gateway)]
        public async Task CanCreateContainer(ConnectionMode connectionMode)
        {
            _output.WriteLine($"Testing with ConnectionMode = '{connectionMode}' ...");

            // Arrange
            CosmosClientOptions clientOptions = new()
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = connectionMode,
                LimitToEndpoint = true,
                RequestTimeout = TimeSpan.FromSeconds(5)
            };

            CosmosClient client = new(ConnectionString, clientOptions);

            Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);

            // Act
            ContainerResponse response = await database.CreateContainerIfNotExistsAsync(ContainerId, PartitionKeyPath);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

            _output.WriteLine($"Created container if it did not exist. StatusCode = {response.StatusCode}.");
        }

        [Theory]
        [InlineData(ConnectionMode.Direct)]
        [InlineData(ConnectionMode.Gateway)]
        public async Task CanCreateItem(ConnectionMode connectionMode)
        {
            _output.WriteLine($"Testing with ConnectionMode = '{connectionMode}' ...");

            // Arrange
            CosmosClientOptions clientOptions = new()
            {
                ConnectionMode = connectionMode,
                LimitToEndpoint = true,
                RequestTimeout = TimeSpan.FromSeconds(5)
            };

            CosmosClient client = new(ConnectionString, clientOptions);

            Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(ContainerId, PartitionKeyPath);

            string itemId = Guid.NewGuid().ToString();

            TestItem item = new()
            {
                Id = itemId,
                Description = "Item created by integration test"
            };

            // Act
            ItemResponse<TestItem> response =
                await container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            _output.WriteLine($"Created item. StatusCode = {response.StatusCode}.");
        }
    }
}
