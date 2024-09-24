using Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
  .AddSingleton(new RabbitMQConfig { QueueName = "invalid_message.queue" })
  .AddSingleton<IRabbitMq, RabbitMQService>()
  .BuildServiceProvider();

var rabbitMq = serviceProvider.GetService<IRabbitMq>();

Console.WriteLine(" [*] Waiting for messages to log...");

rabbitMq.Consume("invalid_message.queue");

await Task.Delay(Timeout.Infinite);