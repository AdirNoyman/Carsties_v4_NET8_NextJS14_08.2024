using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            // var auction = context.Message;

            Console.WriteLine($"Consuming auction created: {context.Message.Id} 😎🤘");
            
            // item = auction
            var item = _mapper.Map<Item>(context.Message);

            if (item.Model == "Foo") throw new ArgumentException($"sell cars with the name of {item.Model}");

            await item.SaveAsync();
        }
    }
   
}