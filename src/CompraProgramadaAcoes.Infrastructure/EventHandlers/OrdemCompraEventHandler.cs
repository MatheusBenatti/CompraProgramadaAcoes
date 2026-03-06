using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.EventHandlers;

public class OrdemCompraEventHandler
{
    private readonly IOrdemCompraRepository _ordemCompraRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<OrdemCompraEventHandler> _logger;

    public OrdemCompraEventHandler(
        IOrdemCompraRepository ordemCompraRepository,
        IMessagePublisher messagePublisher,
        ILogger<OrdemCompraEventHandler> logger)
    {
        _ordemCompraRepository = ordemCompraRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task Handle(CompraConsolidadaEvent @event)
    {
        _logger.LogInformation("Processando compra consolidada para a conta master {ContaMasterId}", @event.ContaMasterId);
        
        // Publicar evento no Kafka para sistemas de execução
        var mensagem = new
        {
            Evento = "CompraConsolidada",
            ContaMasterId = @event.ContaMasterId,
            ComprasRealizadas = @event.ComprasRealizadas.Select(kvp => new
            {
                Ticker = kvp.Key.Valor,
                Quantidade = kvp.Value.Quantidade,
                ValorTotal = kvp.Value.ValorTotal
            }),
            ValorTotalConsolidado = @event.ValorTotalConsolidado.Valor,
            DataCompra = @event.DataCompra,
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
        
        // Publicar evento no Kafka para sistemas de billing e relatórios
        var mensagem = new
        {
            Evento = "InvestimentoRealizado",
            ClienteId = @event.ClienteId,
            ValorInvestido = @event.ValorInvestido.Valor,
            AtivosDistribuidos = @event.AtivosDistribuidos.Select(kvp => new
            {
                Ticker = kvp.Key.Valor,
                Quantidade = kvp.Value
            }),
            DataInvestimento = @event.DataInvestimento,
            Origem = "CompraProgramadaAcoes"
        };

        await _messagePublisher.PublishAsync("investimento-events", 
            System.Text.Json.JsonSerializer.Serialize(mensagem));
        
        _logger.LogInformation("Evento InvestimentoRealizado publicado no Kafka para o cliente {ClienteId}", @event.ClienteId);
    }
}
