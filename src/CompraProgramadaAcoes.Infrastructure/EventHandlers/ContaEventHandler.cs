using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.EventHandlers; 

public class ContaEventHandler(
    IMessagePublisher messagePublisher,
    ILogger<ContaEventHandler> logger)
{
  private readonly IMessagePublisher _messagePublisher = messagePublisher;
  private readonly ILogger<ContaEventHandler> _logger = logger;

  public async Task Handle(AtivosDistribuidosEvent @event)
  {

    // Publicar evento no Kafka para sistemas de integração
    var mensagem = new
    {
      Evento = "AtivosDistribuidos",
      @event.ClienteId,
      @event.NumeroConta,
      AtivosRecebidos = @event.AtivosRecebidos.ToDictionary(
            kvp => kvp.Key.Valor,
            kvp => kvp.Value
        ),
      ValorTotalDistribuido = @event.ValorTotalDistribuido.Valor,
      @event.DataDistribuicao,
      Origem = "CompraProgramadaAcoes"
    };

    await _messagePublisher.PublishAsync("distribuicao-events",
        System.Text.Json.JsonSerializer.Serialize(mensagem));

    _logger.LogInformation("Evento AtivosDistribuidos publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
  }
}
