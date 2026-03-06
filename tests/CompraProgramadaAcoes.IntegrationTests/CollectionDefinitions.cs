using CompraProgramadaAcoes.IntegrationTests.Fixture;

namespace CompraProgramadaAcoes.IntegrationTests;

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection : ICollectionFixture<DatabaseFixture>
{
}
