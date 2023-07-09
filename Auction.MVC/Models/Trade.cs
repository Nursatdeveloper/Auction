using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Auction.MVC.Models {
    public class Trade {
        public long Id { get; set; }
        public long ObjectId { get; set; }
        [EnumDataType(typeof(TradeObject))]
        public TradeType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime AuctionDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Winner { get; set; }
        public long[]? ParticipantIds { get; set; }
        public TradeStatus Status { get; set; }
    }

    public enum TradeType {
        Up,
        Down
    }

    public enum TradeStatus {
        Pending,
        StartedAcceptingParticipants,
        StartedAuction,
        Finished
    }
}
