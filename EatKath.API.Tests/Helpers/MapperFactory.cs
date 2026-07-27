using AutoMapper;
using EatKath.API.Mappings;

namespace EatKath.API.Tests.Helpers
{
    public static class MapperFactory
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            return config.CreateMapper();
        }
    }
}