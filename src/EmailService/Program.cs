using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "rabbitmq" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Main exchange
channel.ExchangeDeclare(exchange: "tour_selection.exchange", 
  type: ExchangeType.Topic);
var queueName = channel.QueueDeclare().QueueName;
channel.QueueBind(queue: queueName,
  exchange: "tour_selection.exchange",
  routingKey: "tour.booked");

// Invalid exchange
channel.ExchangeDeclare(exchange: "invalid.exchange", 
  type: ExchangeType.Topic);
channel.QueueBind(queue: queueName,
  exchange: "invalid.exchange",
  routingKey: "tour.booked");

Console.WriteLine(" [*] Waiting for messages...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
  var body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);
  var routingKey = ea.RoutingKey;

  if (string.IsNullOrEmpty(message))
  {
    channel.BasicPublish(exchange: "invalid.exchange",
      routingKey: routingKey,
      basicProperties: null,
      body: body);

    Console.WriteLine($" [x] Invalid message '{routingKey}':'{message}'");
    return;
  }
  else
  {
    Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
  }
};
channel.BasicConsume(queue: queueName,
  autoAck: true,
  consumer: consumer);

await Task.Delay(Timeout.Infinite);