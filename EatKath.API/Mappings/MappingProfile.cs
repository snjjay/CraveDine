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
using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.DTOs.SurpriseBagReservation;
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
                opt => opt.MapFrom(src => src.Restaurant.Name))
            // Availability is calculated by DealService, not mapped.
            .ForMember(dest => dest.RemainingOffers, opt => opt.Ignore())
            .ForMember(dest => dest.AvailabilityDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsSoldOut, opt => opt.Ignore());

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
                opt => opt.MapFrom(src => src.Restaurant.LogoUrl))
            .ForMember(dest => dest.CoverImageUrl,
                opt => opt.MapFrom(src => src.Restaurant.CoverImageUrl));

        // ============================
        // Redemption
        // ============================

        CreateMap<Redemption, RedemptionDto>()
            .ForMember(dest => dest.DealTitle,
                opt => opt.MapFrom(src => src.Deal.Title))
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.CustomerPhone,
                opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.CustomerEmail,
                opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.RestaurantId,
                opt => opt.MapFrom(src => src.Deal.RestaurantId))
            .ForMember(dest => dest.RestaurantName,
                opt => opt.MapFrom(src => src.Deal.Restaurant.Name))
            .ForMember(dest => dest.CurrencyCode,
                opt => opt.MapFrom(src => src.Deal.Restaurant.CurrencyCode));

        // ============================
        // Reservation
        // ============================

        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.DealTitle,
                opt => opt.MapFrom(src => src.Deal.Title))
            .ForMember(dest => dest.RestaurantName,
                opt => opt.MapFrom(src => src.Deal.Restaurant.Name));

        CreateMap<CreateReservationDto, Reservation>();

        // ============================
        // Surprise Bag
        // ============================

        // RemainingQuantity, IsSoldOut and the reservation counts are
        // calculated by SurpriseBagService, not mapped.
        CreateMap<SurpriseBag, SurpriseBagDto>()
            .ForMember(dest => dest.RestaurantName,
                opt => opt.MapFrom(src => src.Restaurant.Name))
            .ForMember(dest => dest.CurrencyCode,
                opt => opt.MapFrom(src => src.Restaurant.CurrencyCode))
            .ForMember(dest => dest.RemainingQuantity, opt => opt.Ignore())
            .ForMember(dest => dest.IsSoldOut, opt => opt.Ignore());

        CreateMap<SurpriseBag, OwnerSurpriseBagDto>()
            .IncludeBase<SurpriseBag, SurpriseBagDto>()
            .ForMember(dest => dest.ReservedQuantity, opt => opt.Ignore())
            .ForMember(dest => dest.CollectedQuantity, opt => opt.Ignore())
            .ForMember(dest => dest.IsPickupLocked, opt => opt.Ignore());

        CreateMap<CreateSurpriseBagDto, SurpriseBag>();

        // IsActive only when the request sets it (null = unchanged).
        CreateMap<UpdateSurpriseBagDto, SurpriseBag>()
            .ForMember(dest => dest.IsActive, opt =>
            {
                opt.PreCondition(src => src.IsActive.HasValue);
                opt.MapFrom(src => src.IsActive!.Value);
            });

        // ============================
        // Surprise Bag Reservation
        // ============================

        // Bag and restaurant details come from the reservation's own bag;
        // the price is the reserved UnitPrice. CanCancel is calculated by
        // SurpriseBagReservationService.
        CreateMap<SurpriseBagReservation, SurpriseBagReservationDto>()
            .ForMember(dest => dest.BagTitle,
                opt => opt.MapFrom(src => src.SurpriseBag.Title))
            .ForMember(dest => dest.Category,
                opt => opt.MapFrom(src => src.SurpriseBag.Category))
            .ForMember(dest => dest.RestaurantId,
                opt => opt.MapFrom(src => src.SurpriseBag.RestaurantId))
            .ForMember(dest => dest.RestaurantName,
                opt => opt.MapFrom(src => src.SurpriseBag.Restaurant.Name))
            .ForMember(dest => dest.RestaurantAddress,
                opt => opt.MapFrom(src => src.SurpriseBag.Restaurant.Address))
            .ForMember(dest => dest.RestaurantPhone,
                opt => opt.MapFrom(src => src.SurpriseBag.Restaurant.PhoneNumber))
            .ForMember(dest => dest.CurrencyCode,
                opt => opt.MapFrom(src => src.SurpriseBag.Restaurant.CurrencyCode))
            .ForMember(dest => dest.PickupDate,
                opt => opt.MapFrom(src => src.SurpriseBag.PickupDate))
            .ForMember(dest => dest.PickupStartTime,
                opt => opt.MapFrom(src => src.SurpriseBag.PickupStartTime))
            .ForMember(dest => dest.PickupEndTime,
                opt => opt.MapFrom(src => src.SurpriseBag.PickupEndTime))
            .ForMember(dest => dest.TotalPrice,
                opt => opt.MapFrom(src => src.UnitPrice * src.Quantity))
            .ForMember(dest => dest.CanCancel, opt => opt.Ignore());

        CreateMap<SurpriseBagReservation, OwnerSurpriseBagReservationDto>()
            .IncludeBase<SurpriseBagReservation, SurpriseBagReservationDto>()
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.CustomerPhone,
                opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.CustomerEmail,
                opt => opt.MapFrom(src => src.User.Email));
    }
}


// ==========================================================
// MAPPINGS
// ==========================================================
//
// 🔄 Mapping = converting one object type into another.
//
// Mappings folder contains AutoMapper profiles.
//
// MappingProfile.cs
// → General/common mappings
//
// RestaurantProfile.cs
// → Restaurant-related mappings
//
// UserProfile.cs
// → User-related mappings
//
// Example:
//
// CreateUserDto
//      ↓
//   AutoMapper
//      ↓
// User Entity
//
// User Entity
//      ↓
//   AutoMapper
//      ↓
// UserDto
//
// 🔑 Purpose:
// Keep DTO ↔ Entity conversions organised.
// ==========================================================
//So you don't need to study the three files as three different concepts.
//They're all doing the same basic job; they're just organised into different profiles.


//Where Mapping fits in your architecture
//React
// ↓
//Controller
// ↓
//DTO
// ↓
//Validator
// ↓
//Service
// ↓
//🔄 Mapping  ← Mappings folder
// ↓
//Entity
// ↓
//ApplicationDbContext
// ↓
//Database