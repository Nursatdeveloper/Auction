using System.ComponentModel.DataAnnotations;

namespace Auction.MVC.Models {
    public class TradeObject {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        [EnumDataType(typeof(TradeObjectStatus))]
        public TradeObjectStatus Status { get; set; }
    }

    public enum TradeObjectStatus {
        OnTrade,
        Sold,
        NotOnTrade
    }
}
