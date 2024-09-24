using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infrastructure.Messaging;
public class RabbitMQService : IRabbitMq, IDisposable
{
  private readonly IConnection _connection;
  private readonly IModel _channel;

  public RabbitMQService(RabbitMQConfig config)
  {
    var factory = new ConnectionFactory
    {
      HostName = "rabbitmq",
      UserName = "guest",
      Password = "guest"
    };

    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();

    ConfigureQueuesAndExchanges(config.QueueName);
  }

  public void BindToQueue(string queueName, string exchange, string routingKey)
  {
    _channel.QueueBind(queue: queueName,
      exchange: exchange,
      routingKey: routingKey);
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

  public void Consume(string queueName)
  {
    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += (model, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = Encoding.UTF8.GetString(body);
      var routingKey = ea.RoutingKey;

      try
      {
        if (string.IsNullOrEmpty(message))
        {
          throw new Exception("processing error.");
        }
        else
        {
          _channel.BasicAck(deliveryTag: ea.DeliveryTag,
            multiple: false);

          Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
        }
      }
      catch (Exception ex)
      {
        _channel.BasicNack(deliveryTag: ea.DeliveryTag,
          requeue: false,
          multiple: false);

        Console.WriteLine($" [x] Invalid message '{routingKey}':'{message}'");
      }
    };

    _channel.BasicConsume(queue: queueName,
      autoAck: false,
      consumer: consumer);

  }

  public void Dispose()
  {
    _channel?.Dispose();
    _connection?.Dispose();
  }

  private void ConfigureQueuesAndExchanges(string queueName)
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
    
    _channel.QueueDeclare(queue: queueName,
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: new Dictionary<string, object>
      {
        { "x-dead-letter-exchange", "dead_letter.exchange" },
        { "x-dead-letter-routing-key", "" }
      });

    // _channel.QueueBind(queue: "tour_selection.queue",
    //   exchange: "tour_selection.exchange",
    //   routingKey: "tour.*");
  }
}