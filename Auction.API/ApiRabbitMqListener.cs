using Auction.API.Hubs;
using Auction.MVC.Services;
using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Auction.API {

    public interface IAuctionEventHandler {
        Task HandleAuctionStartedEvent(StartAuctionRabbitMqModel model);
        Task HandleEndAuctionEvent(EndAuctionRabbitMqModel model);
    }
    public class AuctionEventHandler: IAuctionEventHandler {
        private readonly IDistributedCache _cache;
        private readonly IRabbitMqService _rabbitMqService;
        public AuctionEventHandler(IDistributedCache cache, IRabbitMqService rabbitMqService) {
            _cache = cache;
            _rabbitMqService= rabbitMqService;
        }
        public async Task HandleAuctionStartedEvent(StartAuctionRabbitMqModel model) {
            var auctionRedisModel = new AuctionRedisModel(model.CurrentPrice, Array.Empty<AuctionHistoryInfo>());
            await _cache.SetRecordAsync<AuctionRedisModel>(model.TradeId.ToString(), auctionRedisModel, TimeSpan.FromDays(1));
        }

        public async Task HandleEndAuctionEvent(EndAuctionRabbitMqModel model) {
            var auctionRedisModel = await _cache.GetRecordAsync<AuctionRedisModel>(model.TradeId.ToString());
            var winner = auctionRedisModel.AuctionHistory.OrderByDescending(x => x.Timestamp).First();
            var results = new TradeResultRabbitMqModel(model.TradeId, winner.SuggestedPrice, winner.Iin);
            _rabbitMqService.SendMessage(results);

        }
    }
    public class ApiRabbitMqListener: BackgroundService {
        private IConnection _connection;
        private IModel _channel;
        private readonly IAuctionEventHandler _eventHandler;
        public ApiRabbitMqListener(IAuctionEventHandler auctionEventHandler) {
            _eventHandler= auctionEventHandler;
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
                switch(rmqMessage.Event) {
                    case RabbitMqEventType.AuctionStarted:
                        await _eventHandler.HandleAuctionStartedEvent(JsonSerializer.Deserialize<StartAuctionRabbitMqModel>(rmqMessage.JsonModel) ?? throw new InvalidDataException());
                        break;
                    default:
                        throw new NotImplementedException();
                }
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

    public class RabbitMqMessage {
        public RabbitMqEventType Event { get; set; }
        public string JsonModel { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum RabbitMqEventType {
        AuctionStarted,
        EndAuction,
        ReceiveTradeResult
    }

    public record StartAuctionRabbitMqModel(long TradeId, decimal CurrentPrice);
    public record EndAuctionRabbitMqModel(long TradeId);
    public record TradeResultRabbitMqModel(long TradeId, decimal SellingPrice, string WinnerIin);




}
