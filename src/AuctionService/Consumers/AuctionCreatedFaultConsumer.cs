using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("==> Consuming faulty auction created 🤨");

            var exception = context.Message.Exceptions.First();

            if (exception.ExceptionType == "System.ArgumentException")
            {
                context.Message.Message.Model = "Foobar";

                await context.Publish(context.Message.Message);
            } else {

                Console.WriteLine("Not an error we know how to handle. Update your error handlers 🤨");
            }
        }
    }
}