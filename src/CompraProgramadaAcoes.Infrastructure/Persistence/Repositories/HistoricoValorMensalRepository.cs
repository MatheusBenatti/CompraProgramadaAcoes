using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Infrastructure.Persistence.Repositories;

public class HistoricoValorMensalRepository(AppDbContext context) : IHistoricoValorMensalRepository
{
    private readonly AppDbContext _context = context;

    public async Task<HistoricoValorMensal> AddAsync(HistoricoValorMensal historico)
    {
        await _context.HistoricoValoresMensais.AddAsync(historico);
        return historico;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
