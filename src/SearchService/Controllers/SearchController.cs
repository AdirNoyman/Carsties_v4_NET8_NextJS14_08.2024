using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            // ORDER BY
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                // Default sort by auction end date, starting with the soonest
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            // FILTER
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                // Return auctions that are ending within the next 6 hours
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                // Deafult => filter by active auctions
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }

            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x => x.Winner == searchParams.Winner);
            }
            
            // PAGING query results
            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {

                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount

            });

        }

    }
}