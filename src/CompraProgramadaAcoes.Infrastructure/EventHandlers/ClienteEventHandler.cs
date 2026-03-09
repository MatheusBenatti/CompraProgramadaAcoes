using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.EventHandlers;

public class ClienteEventHandler(
    IClienteRepository clienteRepository,
    IMessagePublisher messagePublisher,
    ILogger<ClienteEventHandler> logger)
{
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly IMessagePublisher _messagePublisher = messagePublisher;
  private readonly ILogger<ClienteEventHandler> _logger = logger;

  public async Task Handle(ClienteCriadoEvent @event)
  {
    _logger.LogInformation("Processando criação do cliente {ClienteId}", @event.ClienteId);

    // Publicar evento no Kafka para sistemas externos
    var mensagem = new
    {
      Evento = "ClienteCriado",
      @event.ClienteId,
      @event.Nome,
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
    _logger.LogInformation("Cancelando investimentos futuros para cliente {ClienteId} (implementação pendente devido à incompatibilidade de tipos)", @event.ClienteId);

    // Publicar evento no Kafka
    var mensagem = new
    {
      Evento = "ClienteDesativado",
      @event.ClienteId,
      @event.Motivo,
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
    _logger.LogInformation("Recalculando investimentos para cliente {ClienteId} (implementação pendente devido à incompatibilidade de tipos)", @event.ClienteId);

    // Notificar sistemas de billing via Kafka
    var mensagem = new
    {
      Evento = "ValorMensalAlterado",
      @event.ClienteId,
      @event.ValorAnterior,
      @event.ValorNovo,
      DataAlteracao = DateTime.UtcNow,
      Origem = "CompraProgramadaAcoes"
    };

    await _messagePublisher.PublishAsync("billing-events",
        System.Text.Json.JsonSerializer.Serialize(mensagem));

    _logger.LogInformation("Evento ValorMensalAlterado publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
  }
}
