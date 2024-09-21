using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class RabbitMQService : IDisposable
{
  private readonly IConnection _connection;
  private readonly IModel _channel;

  public RabbitMQService(RabbitMQSettings settings)
  {
    var factory = new ConnectionFactory
    {
      HostName = settings.HostName,
      UserName = settings.UserName,
      Password = settings.Password
    };

    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();

    SetupDeadLetterExchange();
  }

  public void Publish(string exchange, string routingKey, string message)
  {
    var body = Encoding.UTF8.GetBytes(message);
    _channel.ExchangeDeclare(exchange, ExchangeType.Topic);

    var args = new Dictionary<string, object>();
    args["x-dead-letter-exchange"] = "dead_letter.exchange";

    _channel.QueueDeclare(queue: "tour_selection.queue",
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: args);

    _channel.BasicPublish(exchange: exchange,
      routingKey: routingKey,
      basicProperties: null,
      body: body);

    Console.WriteLine($"Sent message: {message}");
  }

  public void Subscribe(string queue, Action<string> onMessageReceived)
  {
    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += (model, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = Encoding.UTF8.GetString(body);
      onMessageReceived(message);
    };
    _channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
  }

  private void SetupDeadLetterExchange()
  {
    _channel.ExchangeDeclare(exchange: "dead_letter.exchange", type: ExchangeType.Fanout);
    _channel.QueueDeclare(queue: "dead_letter.queue",
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: null);
    _channel.QueueBind(queue: "dead_letter.queue", exchange: "dead_letter.exchange", routingKey: "");
  }

  public void Dispose()
  {
    _channel?.Dispose();
    _connection?.Dispose();
  }
}