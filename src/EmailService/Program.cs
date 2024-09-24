using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "rabbitmq" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "dead_letter.exchange", 
  type: ExchangeType.Direct);

channel.QueueDeclare(queue: "invalid_message.queue",
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: null);

channel.QueueBind(queue: "invalid_message.queue",
  exchange: "dead_letter.exchange",
  routingKey: "");

channel.ExchangeDeclare(exchange: "tour_selection.exchange", 
  type: ExchangeType.Topic);

channel.QueueDeclare(queue: "emailservice.queue",
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: new Dictionary<string, object>
  {
    { "x-dead-letter-exchange", "dead_letter.exchange" },
    { "x-dead-letter-routing-key", "" }
  });

channel.QueueBind(queue: "emailservice.queue",
  exchange: "tour_selection.exchange",
  routingKey: "tour.booked");

Console.WriteLine(" [*] Waiting for messages...");

var consumer = new EventingBasicConsumer(channel);
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
      channel.BasicAck(deliveryTag: ea.DeliveryTag,
        multiple: false);

      Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
    }
  }
  catch (Exception ex)
  {
    channel.BasicNack(deliveryTag: ea.DeliveryTag,
      requeue: false,
      multiple: false);

    Console.WriteLine($" [x] Invalid message '{routingKey}':'{message}'");
  }
};

channel.BasicConsume(queue: "emailservice.queue",
  autoAck: false,
  consumer: consumer);
  
await Task.Delay(Timeout.Infinite);