using RabbitMQ.Client;
using System.Text;

public class RabbitMQService
{
  private readonly IConnection _connection;
  private readonly IModel _channel;

  public RabbitMQService()
  {
    var factory = new ConnectionFactory
    {
      HostName = "rabbitmq",
      UserName = "guest",
      Password = "guest"
    };

    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();

    ConfigureQueuesAndExchanges();
  }

  public void Publish(string routingKey, string message)
  {
    try
    {
      var body = Encoding.UTF8.GetBytes(message);
      IBasicProperties props = _channel.CreateBasicProperties();
      props.ContentType = "text/plain";
      props.DeliveryMode = 2;
      props.Expiration = "10000";

      _channel.BasicPublish(
        exchange: "tour_selection.exchange",
        routingKey: routingKey,
        basicProperties: null,
        body: body);

      Console.WriteLine($"Sent message: {message}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Failed to publish message: {ex.Message}");
    }
  }

  public void Dispose()
  {
    _channel?.Dispose();
    _connection?.Dispose();
  }

  private void ConfigureQueuesAndExchanges()
  {
    _channel.ExchangeDeclare(exchange: "dead_letter.exchange", 
      type: ExchangeType.Direct);
    
    _channel.QueueDeclare(queue: "invalid_message.queue",
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: null);
    
    _channel.QueueBind(queue: "invalid_message.queue",
      exchange: "dead_letter.exchange",
      routingKey: "");
    
    _channel.ExchangeDeclare(exchange: "tour_selection.exchange", 
      type: ExchangeType.Topic);
    
    _channel.QueueDeclare(queue: "tour_selection.queue",
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: new Dictionary<string, object>
      {
        { "x-dead-letter-exchange", "dead_letter.exchange" },
        { "x-dead-letter-routing-key", "" }
      });

    _channel.QueueBind(queue: "tour_selection.queue",
      exchange: "tour_selection.exchange",
      routingKey: "tour.*");
  }
}