using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Application.EventHandlers;

public class ClienteEventHandler
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<ClienteEventHandler> _logger;

    public ClienteEventHandler(IClienteRepository clienteRepository, ILogger<ClienteEventHandler> logger)
    {
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task Handle(ClienteCriadoEvent @event)
    {
        _logger.LogInformation("Processando criação do cliente {ClienteId}", @event.ClienteId);
        
        // Evento já foi processado durante a criação do aggregate
        // Aqui poderíamos enviar email de boas-vindas, notificações, etc.
        await Task.CompletedTask;
    }

    public async Task Handle(ClienteDesativadoEvent @event)
    {
        _logger.LogInformation("Processando desativação do cliente {ClienteId}: {Motivo}", @event.ClienteId, @event.Motivo);
        
        // Lógica adicional para desativação:
        // - Cancelar investimentos futuros
        // - Notificar sistemas externos
        // - Enviar email de confirmação
        
        await Task.CompletedTask;
    }

    public async Task Handle(ValorMensalAlteradoEvent @event)
    {
        _logger.LogInformation(
            "Processando alteração de valor mensal do cliente {ClienteId}: {ValorAnterior} -> {ValorNovo}",
            @event.ClienteId, @event.ValorAnterior, @event.ValorNovo);
        
        // Lógica adicional:
        // - Recalcular próximos investimentos
        // - Notificar cliente sobre mudança
        // - Atualizar sistemas de billing
        
        await Task.CompletedTask;
    }
}
