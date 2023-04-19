using Azure;
using Newtonsoft.Json.Linq;
using SmartHome.Core.Enums;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Logic
{
    public interface IService
    {
        bool ChangeStatus(int id);
        bool CheckScenario(DeviceDto device, ComparisonOperator op, IEnumerable<PointDto> sensorData, double value);
    }
    public class Services : IService
    {
        private IRepository<Device, DeviceDto> _repository;

        public Services(IRepository<Device, DeviceDto> repository)
        {
            _repository = repository;
        }
        public bool ChangeStatus(int id)
        {
            var device = _repository.GetMapById(id);
            if (device == null) return false;

            device.IsActive = !device.IsActive;
            var updateD = _repository.Update(id, device);

            if (updateD != null)
            {
                return updateD.IsActive;
            }

            return false;
        }

        public bool CheckScenario(DeviceDto? device, ComparisonOperator op, IEnumerable<PointDto>? sensorData, double value)
        {
            if(device == null || sensorData == null) return false;

            switch (op)
            {
                case ComparisonOperator.GreaterThan:
                    if (sensorData.Any(x => x.Value > value))
                    {
                        ChangeStatus(device.Id);

                    }
                    break;
                case ComparisonOperator.LessThan:
                    if (sensorData.Any(x => x.Value < value))
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.Equal:
                    if (sensorData.Any(x => x.Value == value))
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.NotEqual:
                    if (sensorData.Any(x => x.Value != value))
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.GreaterThanOrEqual:
                    if (sensorData.Any(x => x.Value >= value))
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.LessThanOrEqual:
                    if (sensorData.Any(x => x.Value <= value))
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
            }

            return true;
        }
    }
}
