using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class ContaMasterRepository(AppDbContext context) : IContaMasterRepository
{
  private readonly AppDbContext _context = context;

  public async Task<ContaGrafica> ObterContaMasterAsync()
  {
    var conta = await _context.ContasGraficas
        .Include(cg => cg.Custodias)
        .FirstOrDefaultAsync(cg => cg.Tipo == "MASTER");

    if (conta == null)
    {
      // Criar conta master se não existir
      conta = new ContaGrafica(null!); // ClienteId nulo para conta master
      conta.AtualizarTipo("MASTER");
      conta.AtualizarNumeroConta("MST-000001");

      await _context.ContasGraficas.AddAsync(conta);
      await _context.SaveChangesAsync();

      // Recarregar para obter o ID
      conta = await _context.ContasGraficas
          .Include(cg => cg.Custodias)
          .FirstOrDefaultAsync(cg => cg.Tipo == "MASTER");
    }

    return conta!;
  }

  public async Task<ContaGrafica> ObterComCustodiasAsync(long id)
  {
    return await _context.ContasGraficas
        .Include(cg => cg.Custodias)
        .FirstOrDefaultAsync(cg => cg.Id == id && cg.Tipo == "MASTER") ??
          throw new InvalidOperationException("Conta master não encontrada");
  }

  public async Task<List<Custodia>> ObterResiduosAsync()
  {
    return await _context.Custodias
        .Include(c => c.ContaGrafica)
        .Where(c => c.ContaGrafica.Tipo == "MASTER" && c.Quantidade > 0)
        .ToListAsync();
  }

  public async Task AddAsync(ContaGrafica conta)
  {
    await _context.ContasGraficas.AddAsync(conta);
  }

  public async Task UpdateAsync(ContaGrafica conta)
  {
    _context.ContasGraficas.Update(conta);
    await Task.CompletedTask;
  }

  public Task SaveChangesAsync()
  {
    return _context.SaveChangesAsync();
  }
}
