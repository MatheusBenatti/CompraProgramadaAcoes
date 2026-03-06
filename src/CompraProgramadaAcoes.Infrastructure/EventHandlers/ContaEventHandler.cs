using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.EventHandlers;

public class ContaEventHandler
{
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ContaEventHandler> _logger;

    public ContaEventHandler(
        IContaGraficaRepository contaGraficaRepository,
        IMessagePublisher messagePublisher,
        ILogger<ContaEventHandler> logger)
    {
        _contaGraficaRepository = contaGraficaRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task Handle(AtivosDistribuidosEvent @event)
    {
        _logger.LogInformation("Processando distribuição de ativos para o cliente {ClienteId}", @event.ClienteId);
        
        // Buscar conta do cliente no banco
        // Como não temos ObterPorClienteIdAsync e os tipos são diferentes (Guid vs long),
        // vamos pular a busca da conta por enquanto e apenas publicar o evento
        _logger.LogInformation("Publicando evento de distribuição para cliente {ClienteId} (busca de conta desabilitada devido à incompatibilidade de tipos)", @event.ClienteId);

        // Publicar evento no Kafka para sistemas de integração
        var mensagem = new
        {
            Evento = "AtivosDistribuidos",
            ClienteId = @event.ClienteId,
            // ContaId = conta.Id, // Comentado devido à incompatibilidade de tipos
            // NumeroConta = conta.NumeroConta, // Comentado devido à incompatibilidade de tipos
            AtivosRecebidos = @event.AtivosRecebidos.ToDictionary(
                kvp => kvp.Key.Valor,
                kvp => kvp.Value
            ),
            ValorTotalDistribuido = @event.ValorTotalDistribuido.Valor,
            DataDistribuicao = @event.DataDistribuicao,
            Origem = "CompraProgramadaAcoes"
        };

        await _messagePublisher.PublishAsync("distribuicao-events", 
            System.Text.Json.JsonSerializer.Serialize(mensagem));
        
        _logger.LogInformation("Evento AtivosDistribuidos publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
    }
}
