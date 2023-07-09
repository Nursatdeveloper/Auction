using Auction.MVC.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auction.MVC.Jobs {
    public class StartAuctionJob: IJob {
        private readonly IRabbitMqService _rabbitMqService;
        public StartAuctionJob(IRabbitMqService rabbitMqService) {
            _rabbitMqService = rabbitMqService;
        }
        public static string JobQueueName = "StartAuction";
        public string QueueName => "StartAuction";

        public JobFate OnExecute(AuctionDbContext context, string jsonModel) {
            var model = JsonSerializer.Deserialize<StartAuctionInputModel>(jsonModel);
            // EFCORE sucks 
            //var trade = _context.Trades.First(x => x.Id == model.TradeId);
            var trade = DbHelper.GetTradeById(model.TradeId, context.ConnectionString).Result;
            if(trade is null) {
                return new JobFate(JobStatus.Abandoned);
            } else {
                DbHelper.Update("auction.tbtrade", context.ConnectionString, new Dictionary<string, string>() {
                    ["\"Status\""] = "2"
                }, model.TradeId.ToString());
                var tradeObject = context.Objects.Find(trade.ObjectId);
                //trade.Status = Models.TradeStatus.StartedAuction;
                //context.Trades.Update(trade);
                //context.SaveChanges();

                var rmqMessage = new RabbitMqMessage() {
                    Event = RabbitMqEventType.AuctionStarted,
                    JsonModel = JsonSerializer.Serialize(new StartAuctionRabbitMqModel(model.TradeId, tradeObject.Price)),
                    Timestamp = DateTime.Now
                };
                _rabbitMqService.SendMessage(rmqMessage);
                return new JobFate(JobStatus.Finished);
            }
        }

        public record StartAuctionInputModel(long TradeId);
        public record StartAuctionRabbitMqModel(long TradeId, decimal CurrentPrice);
    }

    public class StartAcceptingParticipantsJob: IJob {
        public StartAcceptingParticipantsJob() {

        }
        public string QueueName => "StartAcceptingParticipants";

        public JobFate OnExecute(AuctionDbContext context, string jsonModel) {
            var model = JsonSerializer.Deserialize<StartAcceptingParticipantsInputModel>(jsonModel);
            var trade = context.Trades.Find(model.TradeId);
            if(trade is null) {
                return new JobFate(JobStatus.Abandoned);
            } else {
                trade.Status = Models.TradeStatus.StartedAcceptingParticipants;
                context.Trades.Update(trade);
                context.SaveChanges();
                return new JobFate(JobStatus.Finished);
            }
        }

        public record StartAcceptingParticipantsInputModel(long TradeId);
    }

    public class EndAuctionJob: IJob {
        private readonly IRabbitMqService _rabbitMqService;
        public EndAuctionJob(IRabbitMqService rabbitMqService) {
            _rabbitMqService= rabbitMqService;
        }
        public string QueueName => "EndAuction";
        public static string JobQueueName = "EndAuction";

        public JobFate OnExecute(AuctionDbContext context, string jsonModel) {
            var model = JsonSerializer.Deserialize<EndAuctionInputModel>(jsonModel);
            var rmqMessage = new RabbitMqMessage() {
                Event = RabbitMqEventType.EndAuction,
                JsonModel = JsonSerializer.Serialize(new EndAuctionRabbitMqModel(model.TradeId)),
                Timestamp = DateTime.Now
            };
            _rabbitMqService.SendMessage(rmqMessage);
            return new JobFate(JobStatus.Finished);
        }

        public record EndAuctionInputModel(long TradeId);
        public record EndAuctionRabbitMqModel(long TradeId);
    }

}
