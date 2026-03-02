using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using System.Text.Json;

namespace CompraProgramadaAcoes.Workers;

public class IrEventProcessorWorker : BackgroundService
{
    private readonly ILogger<IrEventProcessorWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public IrEventProcessorWorker(
        ILogger<IrEventProcessorWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        var groupId = _configuration["Kafka:GroupId"] ?? "ir-event-processor-group";
        var topic = _configuration["Kafka:TopicIrEvents"] ?? "ir-events";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topic);

        _logger.LogInformation($"Worker de Processamento de Eventos IR iniciado - Tópico: {topic}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                
                _logger.LogInformation($"Mensagem recebida: {result.Message.Value}");
                
                await ProcessarIrEvent(result.Message.Value);
                
                consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagem do Kafka");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante processamento de evento IR");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        consumer.Close();
        _logger.LogInformation("Worker de Processamento de Eventos IR finalizado");
    }

    private Task ProcessarIrEvent(string mensagem)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var eventoIrRepository = scope.ServiceProvider.GetRequiredService<IEventoIRRepository>();
            
            // Aqui você poderia desserializar e processar o evento
            // Por enquanto, apenas logamos
            _logger.LogInformation($"Processando evento IR: {mensagem}");
            
            // Exemplo de como poderia ser o processamento:
            // var evento = JsonSerializer.Deserialize<IrEventData>(mensagem);
            // await eventoIrRepository.MarcarComoProcessado(evento.Id);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao processar evento IR: {mensagem}");
            return Task.CompletedTask;
        }
    }
}

// DTO para dados do evento IR (exemplo)
public class IrEventData
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal ValorBase { get; set; }
    public decimal ValorIR { get; set; }
    public DateTime DataEvento { get; set; }
}
