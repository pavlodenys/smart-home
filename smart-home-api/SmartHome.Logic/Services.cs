using SmartHome.Core.Enums;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

namespace SmartHome.Logic
{
    public interface IService
    {
        Task<bool> ChangeStatus(int id);
        Task<bool> CheckScenario(DeviceDto device, ComparisonOperator op, IEnumerable<PointDto> sensorData, double value);
    }
    public class Services : IService
    {
        private IRepository<Device, DeviceDto> _repository;

        public Services(IRepository<Device, DeviceDto> repository)
        {
            _repository = repository;
        }
        public async Task<bool> ChangeStatus(int id)
        {
            var device = _repository.GetMapById(id);
            if (device == null) return false;

            device.IsActive = !device.IsActive;
            var updateD = await _repository.Update(id, device);

            if (updateD != null)
            {
                return updateD.IsActive;
            }

            return false;
        }

        public async Task<bool> CheckScenario(DeviceDto? device, ComparisonOperator op, IEnumerable<PointDto>? sensorData, double value)
        {
            if (device == null || sensorData == null)
                return false;

            var deviceStatusChanged = false;

            switch (op)
            {
                case ComparisonOperator.GreaterThan:
                    deviceStatusChanged = sensorData.Any(x => x.Value > value);
                    break;
                case ComparisonOperator.LessThan:
                    deviceStatusChanged = sensorData.Any(x => x.Value < value);
                    break;
                case ComparisonOperator.Equal:
                    deviceStatusChanged = sensorData.Any(x => x.Value == value);
                    break;
                case ComparisonOperator.NotEqual:
                    deviceStatusChanged = sensorData.Any(x => x.Value != value);
                    break;
                case ComparisonOperator.GreaterThanOrEqual:
                    deviceStatusChanged = sensorData.Any(x => x.Value >= value);
                    break;
                case ComparisonOperator.LessThanOrEqual:
                    deviceStatusChanged = sensorData.Any(x => x.Value <= value);
                    break;
            }

            if (deviceStatusChanged)
                return await ChangeStatus(device.Id);

            return false;
        }

    }
}
