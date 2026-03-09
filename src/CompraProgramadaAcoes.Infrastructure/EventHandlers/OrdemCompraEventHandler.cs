using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.EventHandlers;

public class OrdemCompraEventHandler(
    IOrdemCompraRepository ordemCompraRepository,
    IMessagePublisher messagePublisher,
    ILogger<OrdemCompraEventHandler> logger)
{
  private readonly IOrdemCompraRepository _ordemCompraRepository = ordemCompraRepository;
  private readonly IMessagePublisher _messagePublisher = messagePublisher;
  private readonly ILogger<OrdemCompraEventHandler> _logger = logger;

  public async Task Handle(CompraConsolidadaEvent @event)
  {
    _logger.LogInformation("Processando compra consolidada para a conta master {ContaMasterId}", @event.ContaMasterId);

    // Publicar evento no Kafka para sistemas de execução
    var mensagem = new
    {
      Evento = "CompraConsolidada",
      @event.ContaMasterId,
      ComprasRealizadas = @event.ComprasRealizadas.Select(kvp => new
      {
        Ticker = kvp.Key.Valor,
        kvp.Value.Quantidade,
        kvp.Value.ValorTotal
      }),
      ValorTotalConsolidado = @event.ValorTotalConsolidado.Valor,
      @event.DataCompra,
      Status = "Executada",
      Origem = "CompraProgramadaAcoes"
    };

    await _messagePublisher.PublishAsync("compra-events",
        System.Text.Json.JsonSerializer.Serialize(mensagem));

    _logger.LogInformation("Evento CompraConsolidada publicado no Kafka para a conta {ContaMasterId}", @event.ContaMasterId);
  }

  public async Task Handle(InvestimentoRealizadoEvent @event)
  {
    _logger.LogInformation("Processando investimento realizado para o cliente {ClienteId}", @event.ClienteId);

    // Publicar evento no Kafka
    var mensagem = new
    {
      Evento = "InvestimentoRealizado",
      @event.ClienteId,
      ValorInvestido = @event.ValorInvestido.Valor,
      AtivosDistribuidos = @event.AtivosDistribuidos.Select(kvp => new
      {
        Ticker = kvp.Key.Valor,
        Quantidade = kvp.Value
      }),
      @event.DataInvestimento,
      Origem = "CompraProgramadaAcoes"
    };

    await _messagePublisher.PublishAsync("investimento-events",
        System.Text.Json.JsonSerializer.Serialize(mensagem));

    _logger.LogInformation("Evento InvestimentoRealizado publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
  }
}
