using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAllAuctions(string date)
        {    
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                // Get only the newer Auctions (that are later than the date passed in the request)
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

          return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {

            var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            return _mapper.Map<AuctionDto>(auction);

        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);

            //TODO: add current user in the seesion, as seller

            auction.Seller = "testUser";

            _context.Auctions.Add(auction);

            var newAuction = _mapper.Map<AuctionDto>(auction);

            // Publish the new auction to the event bus. If the service buss is down, the new auction will be saved to the Masstransit outbox table and a delivery retry will be activated.
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));           

            // The new auction will be saved to the auction database no matter is it was already published or not
            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to database 😫");

            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, newAuction);   
            

        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();
         
            // TODO: add validation to check if user is the seller

            // If a property is not provided in the request, the current value for that propery property will be used
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Could not save changes to database 😫");


        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            // TODO: Check if the user is the seller (if == userName)

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Could not save changes to database 😫");

        }
}
}