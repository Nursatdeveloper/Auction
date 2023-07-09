using Auction.MVC.Models;

namespace Auction.MVC {
    public record MainPageViewDto(TradeAndObjectDto[] Trades);
    public record TradeAndObjectDto(long TradeId,string Name, string Price, TradeType Type, DateTime AuctionDate, DateTime EndDate, int ParticipantsCount, bool Participate);
    public record TradeViewDto(long TradeId, string Name, string Price, TradeType Type, DateTime StartDate, DateTime AuctionDate, DateTime EndDate, int ParticipantsCount, TradeStatus Status, bool AppliedForParticipation);
    public record ModeratorViewDto(Trade[] Trades, TradeObject[] TradeObjects);
    public record CreateTradeDto(long ObjectId, TradeType Type, DateTime StartDate, DateTime AuctionDate, DateTime EndDate);
    public record CreateTradeObjectDto(string Name, decimal Price, string Description);

    public record RegiserUserDto(string Fio, string Iin, string Password, string Roles, decimal Balance);
    public record LoginUserDto(string Iin, string Password);

    public record AuctionParticipant(string Fio, string Iin, decimal Balance);
    public record AuctionRoomViewDto(TradeViewDto Trade, AuctionParticipant[] Participants);
}
