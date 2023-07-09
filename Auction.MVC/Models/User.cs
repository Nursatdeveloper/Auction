namespace Auction.MVC.Models {
    public class User {
        public long Id { get; set; }
        public string Fio { get; set; }
        public string Iin { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }
        public decimal Balance { get; set; }
        public long[]? TradeIds { get; set; }

    }
}
