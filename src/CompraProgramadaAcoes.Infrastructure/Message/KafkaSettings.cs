namespace CompraProgramadaAcoes.Infrastructure.Message
{
  public class KafkaSettings
  {
    public string BootstrapServers { get; set; } = default!;
    public string GroupId { get; set; } = default!;
    public string Topic { get; set; } = default!;
  }
}
