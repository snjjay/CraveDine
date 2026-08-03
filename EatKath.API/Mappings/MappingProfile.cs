using AutoMapper;
using EatKath.API.DTOs.Area;
using EatKath.API.DTOs.Cuisine;
using EatKath.API.DTOs.Deal;
using EatKath.API.DTOs.DiningType;
using EatKath.API.DTOs.MenuCategory;
using EatKath.API.DTOs.MenuItem;
using EatKath.API.DTOs.Redemption;
using EatKath.API.DTOs.RestaurantImage;
using EatKath.API.DTOs.RestaurantOpeningHour;
using EatKath.API.DTOs.User;
using EatKath.API.DTOs.UserFavorite;
using EatKath.API.Entities;
using EatKath.API.DTOs.Reservation;

namespace EatKath.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ============================
        // User
        // ============================

        CreateMap<User, UserDto>();

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>();

        // ============================
        // Menu Item
        // ============================

        CreateMap<MenuItem, MenuItemDto>();

        CreateMap<CreateMenuItemDto, MenuItem>()
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => false));

        CreateMap<UpdateMenuItemDto, MenuItem>();

        // ============================
        // Menu Category
        // ============================

        CreateMap<MenuCategory, MenuCategoryDto>();
        CreateMap<CreateMenuCategoryDto, MenuCategory>();
        CreateMap<UpdateMenuCategoryDto, MenuCategory>();

        // ============================
        // Area
        // ============================

        CreateMap<Area, AreaDto>();
        CreateMap<CreateAreaDto, Area>();
        CreateMap<UpdateAreaDto, Area>();

        // ============================
        // Cuisine
        // ============================

        CreateMap<Cuisine, CuisineDto>();
        CreateMap<CreateCuisineDto, Cuisine>();
        CreateMap<UpdateCuisineDto, Cuisine>();

        // ============================
        // Dining Type
        // ============================

        CreateMap<DiningType, DiningTypeDto>();
        CreateMap<CreateDiningTypeDto, DiningType>();
        CreateMap<UpdateDiningTypeDto, DiningType>();

        // ============================
        // Deal
        // ============================

        CreateMap<Deal, DealDto>()
            .ForMember(dest => dest.RestaurantName,
                opt => opt.MapFrom(src => src.Restaurant.Name));

        CreateMap<CreateDealDto, Deal>();

        CreateMap<UpdateDealDto, Deal>();

        // ============================
        // Restaurant Image
        // ============================

        CreateMap<RestaurantImage, RestaurantImageDto>();
        CreateMap<CreateRestaurantImageDto, RestaurantImage>();
        CreateMap<UpdateRestaurantImageDto, RestaurantImage>();

        // ============================
        // Restaurant Opening Hour
        // ============================

        CreateMap<RestaurantOpeningHour, RestaurantOpeningHourDto>();
        CreateMap<CreateRestaurantOpeningHourDto, RestaurantOpeningHour>();
        CreateMap<UpdateRestaurantOpeningHourDto, RestaurantOpeningHour>();

        // ============================
        // User Favorite
        // ============================

        CreateMap<UserFavorite, UserFavoriteDto>()
            .ForMember(dest => dest.RestaurantName,
                opt => opt.MapFrom(src => src.Restaurant.Name))
            .ForMember(dest => dest.LogoUrl,
                opt => opt.MapFrom(src => src.Restaurant.LogoUrl));

        // ============================
        // Redemption
        // ============================

        CreateMap<Redemption, RedemptionDto>()
            .ForMember(dest => dest.DealTitle,
                opt => opt.MapFrom(src => src.Deal.Title))
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));

        CreateMap<CreateRedemptionDto, Redemption>();

        // ============================
        // Reservation
        // ============================

        CreateMap<Reservation, ReservationDto>();

        CreateMap<CreateReservationDto, Reservation>();
    }
}