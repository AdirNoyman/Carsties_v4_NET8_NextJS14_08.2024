
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : Profile
    {

        public MappingProfiles()
        {

            CreateMap<Auction, AuctionDto>().IncludeMembers(a => a.Item);
            CreateMap<Item, AuctionDto>();
            CreateMap<CreateAuctionDto, Auction>()
            .ForMember(target => target.Item, source => source.MapFrom(src => src));
            CreateMap<CreateAuctionDto, Item>();
            CreateMap<AuctionDto, AuctionCreated>();
            
        }
        
    }
}