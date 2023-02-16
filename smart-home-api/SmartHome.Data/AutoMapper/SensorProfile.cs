using AutoMapper;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

namespace SmartHome.Data.AutoMapper
{
    public class SensorProfile : Profile
    {
        public SensorProfile()
        {
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

            CreateMap<ScenarioSensorDto, ScenarioSensor>()
   .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
   .ForMember(dto => dto.ScenarioId, opt => opt.MapFrom(entity => entity.ScenarioId))
   .ForMember(dto => dto.SensorId, opt => opt.MapFrom(entity => entity.SensorId))
   .ForMember(dto => dto.Sensor, opt => opt.MapFrom(entity => entity.Sensor));

            CreateMap<ScenarioSensor, ScenarioSensorDto>()
.ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
.ForMember(dto => dto.ScenarioId, opt => opt.MapFrom(entity => entity.ScenarioId))
.ForMember(dto => dto.SensorId, opt => opt.MapFrom(entity => entity.SensorId))
.ForMember(dto => dto.Sensor, opt => opt.MapFrom(entity => entity.Sensor));

            CreateMap<Point, PointDto>()
                 .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
                    .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
                    .ForMember(dto => dto.DateTime, opt => opt.MapFrom(entity => entity.DateTime))
                    .ForMember(dto => dto.Value, opt => opt.MapFrom(entity => entity.Value));

            CreateMap<PointDto, Point>()
              .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
                 .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
                 .ForMember(dto => dto.DateTime, opt => opt.MapFrom(entity => entity.DateTime))
                 .ForMember(dto => dto.Value, opt => opt.MapFrom(entity => entity.Value));

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

            CreateMap<ScenarioDto, Scenario>()
    .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
       .ForMember(dto => dto.SensorValue, opt => opt.MapFrom(entity => entity.SensorValue))
       .ForMember(dto => dto.Operator, opt => opt.MapFrom(entity => entity.Operator))
       .ForMember(dto => dto.Sensors, opt => opt.MapFrom(entity => entity.Sensors))
       .ForMember(dto => dto.Devices, opt => opt.MapFrom(entity => entity.Devices));

            CreateMap<Scenario, ScenarioDto>()
.ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
.ForMember(dto => dto.SensorValue, opt => opt.MapFrom(entity => entity.SensorValue))
.ForMember(dto => dto.Operator, opt => opt.MapFrom(entity => entity.Operator))
.ForMember(dto => dto.Devices, opt => opt.MapFrom(entity => entity.Devices))
.ForMember(dto => dto.Sensors, opt => opt.MapFrom(entity => entity.Sensors));
        }
    }
}
