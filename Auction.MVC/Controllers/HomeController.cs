using Auction.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Auction.MVC.Controllers {
    public class HomeController: Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly AuctionDbContext _context;

        public HomeController(ILogger<HomeController> logger, AuctionDbContext context) {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var iin = User.Claims.FirstOrDefault(x => x.Type == "Iin")?.Value;
            if(iin == null) {
                return RedirectToAction("LoginView", "User");
            }
            var user = _context.Users.First(x => x.Iin == iin);

            var trade = await _context.Trades.ToArrayAsync();
            var tradeAndObjectsDto = trade.Select(x => {
                var tradeObject = _context.Objects.Find(x.ObjectId);
                if(tradeObject != null) {
                    var participate = user.TradeIds?.Contains(x.Id) ?? false;
                    return new TradeAndObjectDto(x.Id, tradeObject.Name, tradeObject.Price.ToString("N0"), x.Type, x.AuctionDate, x.EndDate, x.ParticipantIds?.Length ?? 0, participate);
                } else {
                    throw new InvalidOperationException();
                }
            }).ToArray();

            return View(new MainPageViewDto(tradeAndObjectsDto));
        }

        [HttpGet("{tradeId}")]
        [Route("trade-view")]
        public IActionResult TradeView(long tradeId) {
            var trade = _context.Trades.Find(tradeId);
            if(trade != null) {
                var tradeObject = _context.Objects.Find(trade.ObjectId);
                if(tradeObject != null) {
                    var iin = User.Claims.FirstOrDefault(x => x.Type == "Iin")?.Value;
                    var user = _context.Users.First(x => x.Iin == iin);

                    var dto = new TradeViewDto(trade.Id, tradeObject.Name, tradeObject.Price.ToString("N0"), trade.Type, trade.StartDate, trade.AuctionDate, trade.EndDate, trade.ParticipantIds?.Length ?? 0, trade.Status, user.TradeIds?.Contains(tradeId) ?? false);
                    return View(dto);
                } else {
                    throw new InvalidOperationException();
                }
            } else {
                throw new InvalidOperationException();
            }
        }

        [HttpGet]
        [Route("participate/{iin}/{tradeId}")]
        [Authorize(Policy = "AuthenticatedUsersOnly")]
        public IActionResult ParticipateInAuction(string iin, long tradeId) {
            var user = _context.Users.First(x => x.Iin == iin);
            var trade = _context.Trades.First(x => x.Id  == tradeId);
            var participantIds = new List<long>() { user.Id };
            var tradeIds = new List<long>() { trade.Id };
            if(trade.ParticipantIds != null) {
                trade.ParticipantIds.ToList().ForEach(x => participantIds.Add(x));
            }
            if(user.TradeIds != null) {
                user.TradeIds.ToList().ForEach(x => tradeIds.Add(x));
            }
            user.TradeIds = tradeIds.ToArray();
            trade.ParticipantIds = participantIds.ToArray();
            _context.Trades.Update(trade);
            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("auction-room/{iin}/{tradeId}")]
        [Authorize(Policy = "AuthenticatedUsersOnly")]
        public IActionResult AuctionRoomView(string iin, long tradeId) {
            var trade = _context.Trades.Find(tradeId);
            if(trade != null) {
                var tradeObject = _context.Objects.Find(trade.ObjectId);
                if(tradeObject != null) {
                    var user = _context.Users.First(x => x.Iin == iin);
                    var participants = trade.ParticipantIds.Select(x => {
                        var user = _context.Users.Find(x);
                        return new AuctionParticipant(user.Fio, user.Iin, user.Balance);
                    }).ToArray();
                    var dto = new TradeViewDto(trade.Id, tradeObject.Name, tradeObject.Price.ToString("N0"), trade.Type, trade.StartDate, trade.AuctionDate, trade.EndDate, trade.ParticipantIds?.Length ?? 0, trade.Status, user.TradeIds?.Contains(tradeId) ?? false);
                    return View(new AuctionRoomViewDto(dto, participants));
                } else {
                    throw new InvalidOperationException();
                }
            } else {
                throw new InvalidOperationException();
            }
        }

        [HttpGet]
        [Route("Forbidden")]
        public IActionResult Forbidden() {
            return View();
        }
        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}