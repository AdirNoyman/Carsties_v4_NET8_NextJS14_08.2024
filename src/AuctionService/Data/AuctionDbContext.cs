using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add this 3 tables to the database (used for the masstransit out box functionality)
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}