public interface IMessagePublisher
{
    Task PublishAsync(string topic, string message);
}