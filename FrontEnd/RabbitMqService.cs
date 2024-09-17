using System.Text;
using RabbitMQ.Client;

public class RabbitMQService
{
  private readonly ConnectionFactory Factory;

  public RabbitMQService()
  {
    Factory = new ConnectionFactory
    {
      HostName = "localhost",
      UserName = "guest",
      Password = "guest"
    };
  }

  public void SendMessage(string routingKey, string message)
  {
    using var connection = Factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.ExchangeDeclare("tour_selection.exchange", ExchangeType.Topic);

    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchange: "tour_selection.exchange", 
      routingKey: routingKey, 
      basicProperties: null, 
      body: body);
    
    Console.WriteLine($"Sent message: {message}");
  }
}