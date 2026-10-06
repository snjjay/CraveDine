using AutoMapper;
using EatKath.API.DTOs.Restaurant;
using EatKath.API.Entities;

namespace EatKath.API.Mappings
{
    public class RestaurantProfile : Profile
    {
        public RestaurantProfile()
        {
            // Entity -> DTO
            CreateMap<Restaurant, RestaurantDto>()
                .ForMember(dest => dest.AreaName,
                           opt => opt.MapFrom(src => src.Area.Name))
                .ForMember(dest => dest.Cuisines,
                           opt => opt.MapFrom(src => src.RestaurantCuisines.Select(rc => rc.Cuisine.Name)))
                .ForMember(dest => dest.CuisineIds,
                           opt => opt.MapFrom(src => src.RestaurantCuisines.Select(rc => rc.CuisineId)));

            // Create DTO -> Entity (cuisine links are added by RestaurantService)
            CreateMap<CreateRestaurantDto, Restaurant>();

            // Update DTO -> Entity. A blank LogoUrl (an ordinary profile
            // save that doesn't send one) must not erase the stored logo;
            // logos are managed through the dedicated upload/delete
            // endpoints. Cuisine links are updated by RestaurantService.
            CreateMap<UpdateRestaurantDto, Restaurant>()
                .ForMember(dest => dest.LogoUrl, opt =>
                {
                    opt.Condition(src => !string.IsNullOrWhiteSpace(src.LogoUrl));
                    opt.MapFrom(src => src.LogoUrl);
                });
        }
    }
}