using JetBrains.Annotations;

namespace AzureCosmosEmulatorLinuxTests
{
    [PublicAPI]
    public class TestItem
    {
        private string? _discriminator;

        private string? _id;

        public string Id { get; set; } = null!;

        public string Description { get; set; } = string.Empty;

        public string Discriminator
        {
            get => _discriminator ??= GetType().Name;
            set => _discriminator = value;
        }

        public string PartitionKey { get; set; } = "Default";

#pragma warning disable IDE1006 // Naming Styles

        // ReSharper disable once InconsistentNaming
        public string id
        {
            get => _id ??= $"{Discriminator}|{Id}";
            set => _id = value;
        }

#pragma warning restore IDE1006 // Naming Styles
    }
}
