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

channel.QueueDeclare(queue: "tour_selection.queue",
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: new Dictionary<string, object>
  {
    { "x-dead-letter-exchange", "dead_letter.exchange" },
    { "x-dead-letter-routing-key", "" }
  });

channel.QueueBind(queue: "invalid_message.queue",
  exchange: "tour_selection.exchange",
  routingKey: "");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($" [x] Logged Invalid message:'{message}'");
};

channel.BasicConsume(queue: "invalid_message.queue",
  autoAck: true,
  consumer: consumer);

Console.WriteLine(" [*] Waiting for messages to log...");

await Task.Delay(Timeout.Infinite);