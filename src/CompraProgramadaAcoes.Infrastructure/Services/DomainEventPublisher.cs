using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Domain.Services;
using CompraProgramadaAcoes.Application.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.Services;

public class DomainEventPublisher : IEventPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(IMessagePublisher messagePublisher, ILogger<DomainEventPublisher> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event) where T : IDomainEvent
    {
        await PublishAsync((IDomainEvent)@event);
    }

    public async Task PublishAsync(IDomainEvent @event)
    {
        try
        {
            var topic = ObterTopicoPorEvento(@event.EventType);
            var message = JsonSerializer.Serialize(@event, @event.GetType());
            
            await _messagePublisher.PublishAsync(topic, message);
            
            _logger.LogInformation("Evento {EventType} publicado no tópico {Topic}", @event.EventType, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento {EventType}", @event.EventType);
            throw;
        }
    }

    public async Task PublishBatchAsync(IEnumerable<IDomainEvent> events)
    {
        var tasks = events.Select(PublishAsync);
        await Task.WhenAll(tasks);
    }

    private string ObterTopicoPorEvento(string eventType)
    {
        return eventType switch
        {
            nameof(IrDeduDuroEvent) => "ir-events",
            nameof(IrVendaEvent) => "ir-events",
            nameof(InvestimentoRealizadoEvent) => "investimento-events",
            nameof(CompraConsolidadaEvent) => "compra-events",
            nameof(AtivosDistribuidosEvent) => "distribuicao-events",
            nameof(RebalanceamentoEvent) => "rebalanceamento-events",
            _ => "domain-events"
        };
    }
}
