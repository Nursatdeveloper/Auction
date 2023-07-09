using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auction.API.Hubs {
    public record AuctionRedisModel(decimal CurrentPrice, AuctionHistoryInfo[] AuctionHistory);
    public record AuctionHistoryInfo(string Iin, decimal SuggestedPrice, DateTime Timestamp);
    public class AuctionHub : Hub {
        private readonly IDistributedCache _cache;
        private readonly IDictionary<string, ConnectionParameter> _connections;
        private readonly string _systemUser = "System";
        public AuctionHub(IDistributedCache cache, IDictionary<string, ConnectionParameter> connections) {
            _connections = connections;
            _cache = cache;
        }

        private string getAuctionRoom(long tradeId) {
            return $"auction_{tradeId}";
        }

        public async Task JoinAuction(ConnectionParameter connection) {
            var connectionId = Context.ConnectionId;
            Console.WriteLine("Inside Join Auction with connection " + JsonSerializer.Serialize(connection));
            if(!_connections.ContainsKey(connectionId)) {
                _connections[connectionId] = connection;
                var auctionRoom = getAuctionRoom(connection.TradeId);

                await Groups.AddToGroupAsync(connectionId, auctionRoom);

                await Clients.Group(auctionRoom).SendAsync("ReceiveMessage", _systemUser,
                $"{connection.Fio} {connection.Iin} has joined this auction");

                await SendConnectedParticipants(auctionRoom, connection.TradeId);
            }
        }

        public async Task SendMessage(string message) {
            Console.WriteLine("Sending message =>  " + message);

            if(_connections.TryGetValue(Context.ConnectionId, out var connectionParam)) {
                await Clients.Group(getAuctionRoom(connectionParam.TradeId))
                    .SendAsync("ReceiveMessage", connectionParam.Fio, message);
            }
        }

        public async Task SetIncreaseInfo(string iin, string suggestedPriceStr) {
            var message = $"IIN: {iin} suggested {suggestedPriceStr} at {DateTime.Now}";
            Console.WriteLine(message);
            if(_connections.TryGetValue(Context.ConnectionId, out var connectionParam)) {
                var cacheEntry = await _cache.GetRecordAsync<AuctionRedisModel>(connectionParam.TradeId.ToString());
                decimal suggestedPrice = decimal.Parse(suggestedPriceStr);
                if(suggestedPrice > cacheEntry.CurrentPrice) {
                    var auctionHistory = cacheEntry.AuctionHistory.AddAndReturn(new AuctionHistoryInfo(iin, suggestedPrice, DateTime.Now));
                    await _cache.SetRecordAsync<AuctionRedisModel>(connectionParam.TradeId.ToString(), new AuctionRedisModel(suggestedPrice, auctionHistory));
                    await Clients.Group(getAuctionRoom(connectionParam.TradeId))
                        .SendAsync("ReceiveMessage", connectionParam.Fio, message);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception) {
            if(_connections.TryGetValue(Context.ConnectionId, out var connectionParam)) {
                var auctionRoom = getAuctionRoom(connectionParam.TradeId);
                _connections.Remove(Context.ConnectionId);
                await Clients.Group(auctionRoom)
                    .SendAsync("ReceiveMessage", _systemUser, $"{connectionParam.Fio} has left");

                await SendConnectedParticipants(auctionRoom, connectionParam.TradeId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendConnectedParticipants(string groupName, long tradeId) {
            var users = _connections.Values
                .Where(c => c.TradeId == tradeId)
                .Select(c => new {Iin = c.Iin, Fio = c.Fio});

            await Clients.Group(groupName).SendAsync("ParticipantsInRoom", users);
        }
    }
}
