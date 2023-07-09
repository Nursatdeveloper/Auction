using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Auction.MVC.Jobs {
    public class JobModel {
        [Key]
        [MaxLength(36)] // Guid
        public string JobId { get; set; }
        public string JsonMessage { get; set; }
        public string QueueName { get; set; }
        public DateTime StartAfter { get; set; }
        public DateTime ExpireAt { get; set; }
        public JobStatus Status { get; set; }

    }
    public enum JobStatus {
        Finished,
        Pending,
        Abandoned
    }

    public interface IJob {
        public string QueueName { get; }
        JobFate OnExecute(AuctionDbContext context, string jsonModel);
    }

    public record JobFate(JobStatus Status);

    internal class JobConsumerService: IHostedService, IDisposable {

        private Timer aTimer;
        public static JobModel[] Jobs;
        private AuctionDbContext db;
        private readonly IJob[] _jobCollection;

        public JobConsumerService(AuctionDbContext _db, IJob[] jobCollection) {
            db = _db;
            _jobCollection = jobCollection;
        }

        //This method runs at the start of the application once only as FinalTest was set as Singleton in services
        public Task StartAsync(CancellationToken cancellationToken) {
            aTimer = new Timer(CheckDBEvents, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        //Method runs every TimeSpan.FromSeconds(10)
        private void CheckDBEvents(object state) {
            var jobs = db.Jobs.Where(x => x.Status == JobStatus.Pending && x.StartAfter <= DateTime.Now && x.ExpireAt > DateTime.Now)
                              .OrderBy(x => x.StartAfter)
                              .ToArray();
            if(jobs.Length > 0) {
                foreach(var job in jobs) {
                    var jobConsumer= _jobCollection.Where(x => x.QueueName == job.QueueName).First();
                    var jobFate = jobConsumer.OnExecute(db, job.JsonMessage);
                    if(jobFate.Status == JobStatus.Pending) {
                        job.StartAfter = DateTime.Now.AddMinutes(1);
                    }
                    job.Status = jobFate.Status;
                    db.Jobs.Update(job);
                }
                db.SaveChanges();
            }
        }


        //--------shutdown operations---------//
        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public void Dispose() {
            aTimer?.Dispose();
        }


    }
}

