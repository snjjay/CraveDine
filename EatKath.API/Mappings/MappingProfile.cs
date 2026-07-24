using AutoMapper;
using EatKath.API.DTOs.Area;
using EatKath.API.DTOs.Cuisine;
using EatKath.API.DTOs.DiningType;
using EatKath.API.Entities;
using EatKath.API.DTOs.Deal;

namespace EatKath.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Area, AreaDto>();
        CreateMap<CreateAreaDto, Area>();
        CreateMap<UpdateAreaDto, Area>();


        CreateMap<Cuisine, CuisineDto>();
        CreateMap<CreateCuisineDto, Cuisine>();
        CreateMap<UpdateCuisineDto, Cuisine>();

        CreateMap<DiningType, DiningTypeDto>();
        CreateMap<CreateDiningTypeDto, DiningType>();
        CreateMap<UpdateDiningTypeDto, DiningType>();

        CreateMap<Deal, DealDto>().ReverseMap();
        CreateMap<Deal, CreateDealDto>().ReverseMap();
        CreateMap<Deal, UpdateDealDto>().ReverseMap();
    }
}