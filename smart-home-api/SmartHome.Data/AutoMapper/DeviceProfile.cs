using AutoMapper;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

namespace SmartHome.Data.AutoMapper
{
    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            CreateMap<Device, DeviceDto>()
                          .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
               .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
               .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Description))
               .ForMember(dto => dto.IsActive, opt => opt.MapFrom(entity => entity.IsActive));

            CreateMap<DeviceDto, Device>()
                       .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
            .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
            .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Description))
            .ForMember(dto => dto.IsActive, opt => opt.MapFrom(entity => entity.IsActive));

            CreateMap<ScenarioDeviceDto, ScenarioDevice>()
                       .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
            .ForMember(dto => dto.ScenarioId, opt => opt.MapFrom(entity => entity.ScenarioId))
            .ForMember(dto => dto.DeviceId, opt => opt.MapFrom(entity => entity.DeviceId))
            .ForMember(dto => dto.Device, opt => opt.MapFrom(entity => entity.Device));

            CreateMap<ScenarioDevice, ScenarioDeviceDto>()
                 .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
      .ForMember(dto => dto.ScenarioId, opt => opt.MapFrom(entity => entity.ScenarioId))
      .ForMember(dto => dto.DeviceId, opt => opt.MapFrom(entity => entity.DeviceId))
      .ForMember(dto => dto.Device, opt => opt.MapFrom(entity => entity.Device));
        }
    }
}
