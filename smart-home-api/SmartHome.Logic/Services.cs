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
        bool CheckScenario(Device device, ComparisonOperator op, double sensorData, double value);
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

            return updateD.IsActive;
        }

        public bool CheckScenario(Device device, ComparisonOperator op, double sensorData, double value)
        {
            switch (op)
            {
                case ComparisonOperator.GreaterThan:
                    if (sensorData > value)
                    {
                        ChangeStatus(device.Id);

                    }
                    break;
                case ComparisonOperator.LessThan:
                    if (sensorData < value)
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.Equal:
                    if (sensorData == value)
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.NotEqual:
                    if (sensorData != value)
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.GreaterThanOrEqual:
                    if (sensorData >= value)
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
                case ComparisonOperator.LessThanOrEqual:
                    if (sensorData <= value)
                    {
                        ChangeStatus(device.Id);
                    }
                    break;
            }

            return true;
        }
    }
}
