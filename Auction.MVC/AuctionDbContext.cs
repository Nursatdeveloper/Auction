using Auction.MVC.Jobs;
using Auction.MVC.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auction.MVC {
    public class AuctionDbContext: DbContext {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options, string connection) {
            connectionString = connection;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        private string connectionString;
        public string ConnectionString => connectionString;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {

            if(connectionString != null) {
                var config = connectionString;
                optionsBuilder.UseNpgsql(config);
            }

            base.OnConfiguring(optionsBuilder);
        }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeObject> Objects { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<JobModel> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Trade>()
                .ToTable("tbtrade", schema: "auction");

            modelBuilder.Entity<TradeObject>()
                .ToTable("tbtradeobject", schema: "auction");

            modelBuilder.Entity<User>()
                .ToTable("tbuser", schema: "users");

            modelBuilder.Entity<JobModel>()
                .ToTable("tbjob", schema: "jobs");
        }
    }
}
