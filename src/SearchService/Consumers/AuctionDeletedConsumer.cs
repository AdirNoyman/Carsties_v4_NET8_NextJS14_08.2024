using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine($"Consuming delete auction: {context.Message.Id} 😈🤘");

            var resultOfDeletion = await DB.DeleteAsync<Item>(context.Message.Id);

            if (!resultOfDeletion.IsAcknowledged) throw new MessageException(typeof(AuctionDeleted), "Problem deleting auction from mongoDb 😫");
        }
    }
}