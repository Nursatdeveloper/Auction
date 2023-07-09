using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using Auction.MVC.Services;

namespace Auction.MVC {
    public interface IAuctionEventHandler {
        Task HandleTradeResult(TradeResultRabbitMqModel model);
    }
    public class AuctionEventHandler: IAuctionEventHandler {
        private readonly AuctionDbContext _context;
        public AuctionEventHandler(AuctionDbContext context) {
            _context = context;
        }
        public async Task HandleTradeResult(TradeResultRabbitMqModel model) {
            var trade = await _context.Trades.FindAsync(model.TradeId);
            trade.Winner = model.WinnerIin;
            var tradeObject = await _context.Objects.FindAsync(trade.ObjectId);
            tradeObject.Price = model.SellingPrice;
            _context.UpdateRange(tradeObject, trade);
            await _context.SaveChangesAsync();
        }
    }
    public class WebRabbitMqListener: BackgroundService {
        private IConnection _connection;
        private IModel _channel;
        private const string connectionString = "Host=localhost;Port=5432;Database=dbAuction;User Id=postgres;Password=Northernlights2010";
        public WebRabbitMqListener() {
            var factory = new ConnectionFactory { HostName = "localhost", Port = 5672 };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "AuctionQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (channel, eventArgs) => {
                var content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var rmqMessage = JsonSerializer.Deserialize<RabbitMqMessage>(content);
                Console.WriteLine($"RabbitMqListener => Received Message {rmqMessage}");
                var model = JsonSerializer.Deserialize<TradeResultRabbitMqModel>(rmqMessage.JsonModel);
                var trade = await DbHelper.GetTradeById(model.TradeId, connectionString);
                var tradeObject = await DbHelper.GetTradeObjectById(trade.ObjectId, connectionString);
                DbHelper.Update("auction.tbtrade", connectionString, new Dictionary<string, string>() {
                    ["\"Winner\""] = $"'{model.WinnerIin}'",
                    ["\"Status\""] = "3",
                }, model.TradeId.ToString());

                DbHelper.Update("auction.tbtradeobject", connectionString, new Dictionary<string, string>() {
                    ["\"Price\""] = model.SellingPrice.ToString(),
                    ["\"Status\""] = "1",
                }, tradeObject.Id.ToString());

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };
            _channel.BasicConsume("AuctionQueue", false, consumer);

            return Task.CompletedTask;
        }
        public override void Dispose() {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

       
    }

    public record TradeResultRabbitMqModel(long TradeId, decimal SellingPrice, string WinnerIin);
}
