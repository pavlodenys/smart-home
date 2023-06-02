using AutoMapper;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

namespace SmartHome.Data.AutoMapper
{
    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            CreateMap<Device, DeviceDto>();
            CreateMap<DeviceDto, Device>();
            CreateMap<ScenarioDeviceDto, ScenarioDevice>();
            CreateMap<ScenarioDevice, ScenarioDeviceDto>();
            CreateMap<ScenarioDto, Scenario>();
            CreateMap<Scenario, ScenarioDto>();
            CreateMap<ScenarioSensorDto, ScenarioSensor>();
            CreateMap<ScenarioSensor, ScenarioSensorDto>();
            CreateMap<Point, PointDto>();
            CreateMap<PointDto, Point>();
            CreateMap<HomeUser, HomeUserDto>();
            CreateMap<HomeUserDto, HomeUser>();
            CreateMap<RefreshToken, RefreshTokenDto>();
            CreateMap<RefreshTokenDto, RefreshToken>();

            CreateMap<Sensor, SensorDto>()
        .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
        .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
        .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Description))
        .ForMember(dto => dto.Type, opt => opt.MapFrom(entity => entity.Type))
        .ForMember(dto => dto.ChartData, opt => opt.MapFrom(entity => entity.Data));

            CreateMap<SensorDto, Sensor>()
               .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
               .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Description))
               .ForMember(dto => dto.Type, opt => opt.MapFrom(entity => entity.Type))
               .ForMember(dto => dto.Data, opt => opt.MapFrom(entity => entity.ChartData));



            CreateMap<Entities.Data, ChartDataDto>()
            .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
               .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
               .ForMember(dto => dto.Data, opt => opt.MapFrom(entity => entity.Points))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Description));

            CreateMap<ChartDataDto, Entities.Data>()
         .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
            .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
            .ForMember(dto => dto.Points, opt => opt.MapFrom(entity => entity.Data))
            .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Description));
        }
    }
}
