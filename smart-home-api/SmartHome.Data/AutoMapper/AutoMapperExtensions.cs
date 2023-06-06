using AutoMapper;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

namespace SmartHome.Data.AutoMapper
{
    public static class AutoMapperExtensions
    {
        private static IMapper? Mapper { get; set; }

        public static IMapper GetMapper()
        {
            // Access the AutoMapper configuration from the other project
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Point, PointDto>();
                cfg.CreateMap<PointDto, Point>();
                // Add any other profiles or custom mappings you have in the other project
            });

            // Build and return the IMapper instance
            return configuration.CreateMapper();
        }
        public static TDto MapToDto<TEntity, TDto>(this TEntity entity)
        {
            if (Mapper == null)
            {
                Mapper = GetMapper();
            }
            return Mapper.Map<TDto>(entity);
        }
    }
}
