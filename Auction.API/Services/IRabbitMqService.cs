using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Auction.MVC.Services
{
    public interface IRabbitMqService
    {
        void SendMessage(object obj);
        void SendMessage(string message);
    }

    public class RabbitMqService: IRabbitMqService {
        public void SendMessage(object obj) {
            var message = JsonSerializer.Serialize(obj);
            Console.WriteLine($"RabbitMqService => Sending Message {message}");
            SendMessage(message);
        }

        public void SendMessage(string message) {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel()) {
                channel.QueueDeclare(queue: "AuctionQueue",
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                               routingKey: "AuctionQueue",
                               basicProperties: null,
                               body: body);
            }
        }
    }
}
