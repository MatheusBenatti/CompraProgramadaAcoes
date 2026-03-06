using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using CompraProgramadaAcoes.IntegrationTests.Fixture;

namespace CompraProgramadaAcoes.IntegrationTests.Infrastructure;

[Collection("Sequential")]
public class RepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly ContaMasterRepository _repository;

    public RepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(_fixture.ConnectionString, ServerVersion.AutoDetect(_fixture.ConnectionString))
            .Options;
            
        _context = new AppDbContext(options);
        _repository = new ContaMasterRepository(_context);
    }

    [Fact]
    public async Task ObterContaMaster_ShouldReturnOrCreateMasterAccount()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // Act
        var contaMaster = await _repository.ObterContaMasterAsync();

        // Assert
        contaMaster.Should().NotBeNull();
        contaMaster.Tipo.Should().Be("MASTER");
        contaMaster.NumeroConta.Should().StartWith("MST-");
    }

    [Fact]
    public async Task ObterResiduos_ShouldReturnEmptyWhenNoCustodiasExist()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // Act
        var residuos = await _repository.ObterResiduosAsync();

        // Assert
        residuos.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateContaMaster_ShouldPersistChanges()
    {
        // Arrange
        var conta = await _repository.ObterContaMasterAsync();
        var originalNumero = conta.NumeroConta;

        // Act
        conta.AtualizarNumeroConta("UPDATED-NUMBER");
        await _repository.UpdateAsync(conta);
        await _repository.SaveChangesAsync();

        var updatedConta = await _repository.ObterComCustodiasAsync(conta.Id);

        // Assert
        updatedConta.NumeroConta.Should().Be("UPDATED-NUMBER");
        updatedConta.NumeroConta.Should().NotBe(originalNumero);
    }
}
