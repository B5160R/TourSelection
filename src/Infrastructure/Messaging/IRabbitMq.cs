namespace Infrastructure.Messaging;
public interface IRabbitMq
{
  void BindToQueue(string queueName, string exchange, string routingKey);
  void Publish(string routingKey, string message);
  void Consume(string queueName);
}