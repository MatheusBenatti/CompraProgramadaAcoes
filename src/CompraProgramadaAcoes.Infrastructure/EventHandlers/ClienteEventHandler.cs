using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.EventHandlers;

public class ClienteEventHandler
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ClienteEventHandler> _logger;

    public ClienteEventHandler(
        IClienteRepository clienteRepository, 
        IMessagePublisher messagePublisher,
        ILogger<ClienteEventHandler> logger)
    {
        _clienteRepository = clienteRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task Handle(ClienteCriadoEvent @event)
    {
        _logger.LogInformation("Processando criação do cliente {ClienteId}", @event.ClienteId);
        
        // Publicar evento no Kafka para sistemas externos
        var mensagem = new
        {
            Evento = "ClienteCriado",
            ClienteId = @event.ClienteId,
            Nome = @event.Nome,
            CPF = @event.Cpf.Valor,
            Email = @event.Email.Valor,
            DataCriacao = @event.DataAdesao,
            Origem = "CompraProgramadaAcoes"
        };

        await _messagePublisher.PublishAsync("cliente-events", 
            System.Text.Json.JsonSerializer.Serialize(mensagem));
        
        _logger.LogInformation("Evento ClienteCriado publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
    }

    public async Task Handle(ClienteDesativadoEvent @event)
    {
        _logger.LogInformation("Processando desativação do cliente {ClienteId}: {Motivo}", @event.ClienteId, @event.Motivo);
        
        // Cancelar investimentos futuros no banco
        // Como o ID é Guid vs long, vamos apenas logar por enquanto
        _logger.LogInformation("Cancelando investimentos futuros para cliente {ClienteId} (implementação pendente devido à incompatibilidade de tipos)", @event.ClienteId);
        // var cliente = await _clienteRepository.GetByIdAsync(@event.ClienteId);
        // Implementação futura quando os tipos forem compatíveis

        // Publicar evento no Kafka
        var mensagem = new
        {
            Evento = "ClienteDesativado",
            ClienteId = @event.ClienteId,
            Motivo = @event.Motivo,
            DataDesativacao = DateTime.UtcNow,
            Origem = "CompraProgramadaAcoes"
        };

        await _messagePublisher.PublishAsync("cliente-events", 
            System.Text.Json.JsonSerializer.Serialize(mensagem));
        
        _logger.LogInformation("Evento ClienteDesativado publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
    }

    public async Task Handle(ValorMensalAlteradoEvent @event)
    {
        _logger.LogInformation(
            "Processando alteração de valor mensal do cliente {ClienteId}: {ValorAnterior} -> {ValorNovo}",
            @event.ClienteId, @event.ValorAnterior, @event.ValorNovo);
        
        // Recalcular próximos investimentos
        // Como o ID é Guid vs long, vamos apenas logar por enquanto
        _logger.LogInformation("Recalculando investimentos para cliente {ClienteId} (implementação pendente devido à incompatibilidade de tipos)", @event.ClienteId);
        // var cliente = await _clienteRepository.GetByIdAsync(@event.ClienteId);
        // Implementação futura quando os tipos forem compatíveis

        // Notificar sistemas de billing via Kafka
        var mensagem = new
        {
            Evento = "ValorMensalAlterado",
            ClienteId = @event.ClienteId,
            ValorAnterior = @event.ValorAnterior,
            ValorNovo = @event.ValorNovo,
            DataAlteracao = DateTime.UtcNow,
            Origem = "CompraProgramadaAcoes"
        };

        await _messagePublisher.PublishAsync("billing-events", 
            System.Text.Json.JsonSerializer.Serialize(mensagem));
        
        _logger.LogInformation("Evento ValorMensalAlterado publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
    }
}
