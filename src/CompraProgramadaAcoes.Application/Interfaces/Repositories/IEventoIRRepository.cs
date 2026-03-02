using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IEventoIRRepository
{
    Task<EventoIR?> ObterPorIdAsync(long id);
    Task<List<EventoIR>> ObterPorClienteAsync(long clienteId);
    Task<List<EventoIR>> ObterNaoPublicadosAsync();
    Task<EventoIR> AddAsync(EventoIR evento);
    Task UpdateAsync(EventoIR evento);
    Task<int> SaveChangesAsync();
}
