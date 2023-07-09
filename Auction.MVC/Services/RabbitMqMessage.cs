namespace Auction.MVC.Services {
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
}
