using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace AzureCosmosEmulatorLinuxTests
{
    public class CosmosClientTests
    {
        private const string ConnectionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private const string DatabaseId = "TestDatabase";

        private const string ContainerId = "TestContainer";

        private const string PartitionKeyPath = "/" + nameof(TestItem.PartitionKey);

        [Fact]
        public async Task CanConnectToAzureCosmosEmulator()
        {
            // Arrange
            CosmosClient client = new(ConnectionString);

            // Act
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }

        [Fact]
        public async Task CanCreateContainer()
        {
            // Arrange
            CosmosClient client = new(ConnectionString);
            Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);

            // Act
            ContainerResponse response = await database.CreateContainerIfNotExistsAsync(ContainerId, PartitionKeyPath);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }

        [Fact]
        public async Task CanCreateItem()
        {
            // Arrange
            CosmosClient client = new(ConnectionString);
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
        }
    }
}
