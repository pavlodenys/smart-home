using Moq;
using NUnit.Framework;
using SmartHome.Core.Enums;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;

namespace SmartHome.Tests
{
    [TestFixture]
    public class ServicesTests
    {
        private Mock<IRepository<Device, DeviceDto>> _repositoryMock;
        private Services _services;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IRepository<Device, DeviceDto>>();
            _services = new Services(_repositoryMock.Object);
        }

        [Test]
        public async Task ChangeStatus_DeviceExists_StatusToggled()
        {
            // Arrange
            var deviceId = 1;
            var deviceDto = new DeviceDto { Id = deviceId, IsActive = false };
            _repositoryMock.Setup(r => r.GetMapById(deviceId)).Returns(deviceDto);
           // _repositoryMock.Setup(r => r.Update(deviceId, It.IsAny<DeviceDto>())).Returns(deviceDto);

            // Act
            var result = await _services.ChangeStatus(deviceId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(deviceDto.IsActive);
            _repositoryMock.Verify(r => r.Update(deviceId, It.IsAny<DeviceDto>()), Times.Once);
        }

        [Test]
        public async Task ChangeStatus_DeviceNotFound_ReturnsFalse()
        {
            // Arrange
            var deviceId = 1;
           // _repositoryMock.Setup(r => r.GetMapById(deviceId)).Returns((DeviceDto)null);

            // Act
            var result = await _services.ChangeStatus(deviceId);

            // Assert
            Assert.IsFalse(result);
            _repositoryMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<DeviceDto>()), Times.Never);
        }

        [TestCase(ComparisonOperator.GreaterThan, 3, 5, true)]
        [TestCase(ComparisonOperator.LessThan, 5, 3, true)]
        [TestCase(ComparisonOperator.Equal, 5, 5, true)]
        [TestCase(ComparisonOperator.NotEqual, 5, 3, true)]
        [TestCase(ComparisonOperator.GreaterThanOrEqual, 5, 5, true)]
        [TestCase(ComparisonOperator.LessThanOrEqual, 5, 3, true)]
        [TestCase(ComparisonOperator.GreaterThan, 5, 3, false)]
        [TestCase(ComparisonOperator.LessThan, 3, 5, false)]
        [TestCase(ComparisonOperator.Equal, 5, 3, false)]
        [TestCase(ComparisonOperator.NotEqual, 5, 5, false)]
        [TestCase(ComparisonOperator.GreaterThanOrEqual, 5, 3, false)]
        [TestCase(ComparisonOperator.LessThanOrEqual, 3, 5, false)]
        public void CheckScenario_ValidData_DeviceStatusChanged(ComparisonOperator op, double value, double sensorValue, bool expectedStatus)
        {
            // Arrange
            var deviceId = 1;
            var deviceDto = new DeviceDto { Id = deviceId };
            var sensorData = new List<PointDto> { new PointDto { Value = sensorValue } };
            _repositoryMock.Setup(r => r.GetMapById(deviceId)).Returns(deviceDto);
           // _repositoryMock.Setup(r => r.Update(deviceId, It.IsAny<DeviceDto>())).Returns(deviceDto);

            // Act
            var result = _services.CheckScenario(deviceDto, op, sensorData, value);

            // Assert
            //Assert.IsTrue(result);
            Assert.That(deviceDto.IsActive, Is.EqualTo(expectedStatus));
            //_repositoryMock.Verify(r => r.Update(deviceId, It.IsAny<DeviceDto>()), Times.Once);
        }

        [Test]
        public async Task CheckScenario_NullDeviceOrSensorData_ReturnsFalse()
        {
            // Arrange
            var deviceDto = new DeviceDto();
            var sensorData = new List<PointDto>();

            // Act
            var result = await _services.CheckScenario(null, ComparisonOperator.GreaterThan, sensorData, 5);
            var result2 = await _services.CheckScenario(deviceDto, ComparisonOperator.GreaterThan, null, 5);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(result2);
            _repositoryMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<DeviceDto>()), Times.Never);
        }
    }
}
