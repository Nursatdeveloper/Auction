using Auction.MVC.Jobs;
using Auction.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Auction.MVC.Jobs.EndAuctionJob;
using static Auction.MVC.Jobs.StartAcceptingParticipantsJob;
using static Auction.MVC.Jobs.StartAuctionJob;

namespace Auction.MVC.Controllers {
    [Controller]
    [Route("moderator")]
    [Authorize(Policy = "ModeratorOnly")]
    public class ModeratorController : Controller {
        private readonly AuctionDbContext _context;
        public ModeratorController(AuctionDbContext context) {
            _context= context;
        }

        [HttpGet]
        [Route("view")]
        public async Task<IActionResult> Index() {
            var tradeList = await _context.Trades.ToArrayAsync();
            var tradeObjectList = await _context.Objects.ToArrayAsync();
            var moderatorViewDto = new ModeratorViewDto(tradeList, tradeObjectList);
            return View(moderatorViewDto);
        }

        [HttpGet]
        [Route("create-trade")]
        public IActionResult CreateTradeView() {
            return View();
        }

        [HttpPost]
        [Route("create-trade")]

        public async Task<IActionResult> CreateTradePost(CreateTradeDto createTradeDto) {
            var trade = new Trade() {
                Id = new IdGenerator().GetId(),
                ObjectId = createTradeDto.ObjectId,
                Type = createTradeDto.Type,
                AuctionDate = createTradeDto.AuctionDate,
                EndDate = createTradeDto.EndDate,
                StartDate = createTradeDto.StartDate,
                Status = TradeStatus.Pending
            };

            var tradeObject = await _context.Objects.FirstAsync(x => x.Id == createTradeDto.ObjectId);
            tradeObject.Status = TradeObjectStatus.OnTrade;

            _context.Objects.Update(tradeObject);
            var createdTrade = _context.Trades.Add(trade);

            var startAcceptingParticipantsJob = new JobModel() {
                JobId = Guid.NewGuid().ToString(),
                JsonMessage = JsonSerializer.Serialize(new StartAcceptingParticipantsInputModel(trade.Id)),
                QueueName = new StartAcceptingParticipantsJob().QueueName,
                StartAfter = createTradeDto.StartDate,
                ExpireAt = createTradeDto.StartDate.AddDays(1),
                Status = JobStatus.Pending
            };

            var startAuctionJob = new JobModel() {
                JobId = Guid.NewGuid().ToString(),
                JsonMessage = JsonSerializer.Serialize(new StartAuctionInputModel(trade.Id)),
                QueueName =  StartAuctionJob.JobQueueName,
                StartAfter = createTradeDto.AuctionDate,
                ExpireAt = createTradeDto.AuctionDate.AddDays(1),
                Status = JobStatus.Pending
            };

            var endAuctionJob = new JobModel() {
                JobId = Guid.NewGuid().ToString(),
                JsonMessage = JsonSerializer.Serialize(new EndAuctionInputModel(trade.Id)),
                QueueName = EndAuctionJob.JobQueueName,
                StartAfter = createTradeDto.EndDate,
                ExpireAt = createTradeDto.EndDate.AddDays(1),
                Status = JobStatus.Pending
            };

            await _context.Jobs.AddRangeAsync(startAcceptingParticipantsJob, startAuctionJob, endAuctionJob);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("create-object")]
        public IActionResult CreateTradeObjectView() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTradeObjectPost(CreateTradeObjectDto createTradeObjectDto) {
            var tradeObject = new TradeObject() {
                Name = createTradeObjectDto.Name,
                Price = createTradeObjectDto.Price,
                Description= createTradeObjectDto.Description,
                Status = TradeObjectStatus.NotOnTrade
            };

            _context.Objects.Add(tradeObject);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }

}
