using AutoMapper;
using EatKath.API.DTOs.Area;
using EatKath.API.DTOs.Cuisine;
using EatKath.API.DTOs.Deal;
using EatKath.API.DTOs.DiningType;
using EatKath.API.DTOs.MenuCategory;
using EatKath.API.Entities;
using EatKath.API.DTOs.User;
using EatKath.API.DTOs.MenuItem;
using EatKath.API.DTOs.RestaurantImage;
using EatKath.API.DTOs.RestaurantOpeningHour;


namespace EatKath.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<MenuItem, MenuItemDto>();

        CreateMap<CreateMenuItemDto, MenuItem>()
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => false));

        CreateMap<UpdateMenuItemDto, MenuItem>();



        CreateMap<CreateUserDto, User>()
        .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>();

        CreateMap<CreateMenuCategoryDto, MenuCategory>();
        CreateMap<UpdateMenuCategoryDto, MenuCategory>();
        CreateMap<MenuCategory, MenuCategoryDto>();


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

        CreateMap<RestaurantImage, RestaurantImageDto>();
        CreateMap<CreateRestaurantImageDto, RestaurantImage>();
        CreateMap<UpdateRestaurantImageDto, RestaurantImage>();


        CreateMap<RestaurantOpeningHour, RestaurantOpeningHourDto>();
        CreateMap<CreateRestaurantOpeningHourDto, RestaurantOpeningHour>();
        CreateMap<UpdateRestaurantOpeningHourDto, RestaurantOpeningHour>();
    }
}