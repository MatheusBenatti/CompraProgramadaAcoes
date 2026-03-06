using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class EventoIRRepository(AppDbContext context) : IEventoIRRepository
{
  private readonly AppDbContext _context = context;

  public async Task<EventoIR?> ObterPorIdAsync(long id)
  {
    return await _context.EventosIR
        .Include(e => e.Cliente)
        .FirstOrDefaultAsync(e => e.Id == id);
  }

  public async Task<List<EventoIR>> ObterPorClienteAsync(long clienteId)
  {
    return await _context.EventosIR
        .Where(e => e.ClienteId == clienteId)
        .OrderByDescending(e => e.DataEvento)
        .ToListAsync();
  }

  public async Task<List<EventoIR>> ObterNaoPublicadosAsync()
  {
    return await _context.EventosIR
        .Where(e => !e.PublicadoKafka)
        .OrderBy(e => e.DataEvento)
        .ToListAsync();
  }

  public async Task<List<EventoIR>> ObterPorDataReferenciaAsync(DateTime dataReferencia)
  {
    var inicio = dataReferencia.Date;
    var fim = dataReferencia.Date.AddDays(1).AddTicks(-1);

    return await _context.EventosIR
        .Include(e => e.Cliente)
        .Where(e => e.DataEvento >= inicio && e.DataEvento <= fim)
        .OrderByDescending(e => e.DataEvento)
        .ToListAsync();
  }

  public async Task<EventoIR> AddAsync(EventoIR evento)
  {
    await _context.EventosIR.AddAsync(evento);
    return evento;
  }

  public async Task UpdateAsync(EventoIR evento)
  {
    _context.EventosIR.Update(evento);
    await Task.CompletedTask;
  }

  public async Task<int> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync();
  }
}
