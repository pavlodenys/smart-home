using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SmartHome.Data;
using SmartHome.Logic;

namespace SmartHome.Tests
{
    public class RepositoryTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            // Create a mock of the IMapper interface
            var mapperMock = new Mock<IMapper>();
            _mapper = mapperMock.Object;
        }

        //[Test]
        public async Task GetById_ShouldReturnDto_WhenEntityExists()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<SmartHomeDbContext>()
                //.UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var dbContext = new SmartHomeDbContext(dbContextOptions);
            var repository = new Repository<SomeEntity, SomeDto>(dbContext, _mapper);

            // Create a sample entity and add it to the in-memory database
            var entity = new SomeEntity { Id = 1, Name = "Test" };
            dbContext.Set<SomeEntity>().Add(entity);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetById(e => e.Id == 1);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Name, Is.EqualTo("Test"));
        }

        //[Test]
        public async Task GetById_ShouldReturnNull_WhenEntityDoesNotExist()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<SmartHomeDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var dbContext = new SmartHomeDbContext(dbContextOptions);
            var repository = new Repository<SomeEntity, SomeDto>(dbContext, _mapper);

            // Act
            var result = await repository.GetById(e => e.Id == 1);

            // Assert
            Assert.Null(result);
        }

        //[Test]
        public async Task Create_ShouldAddEntityToDatabaseAndReturnDto()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<SmartHomeDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var dbContext = new SmartHomeDbContext(dbContextOptions);
            var repository = new Repository<SomeEntity, SomeDto>(dbContext, _mapper);

            // Create a sample dto
            var dto = new SomeDto { Id = 1, Name = "Test" };

            // Act
            var result = await repository.Create(dto);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Id, Is.EqualTo(dto.Id));
            Assert.That(result.Name, Is.EqualTo(dto.Name));

            // Check if the entity is added to the in-memory database
            var entity = dbContext.Set<SomeEntity>().Find(dto.Id);
            Assert.NotNull(entity);
            Assert.That(entity.Name, Is.EqualTo(dto.Name));
        }
    }

    public class SomeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class SomeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
