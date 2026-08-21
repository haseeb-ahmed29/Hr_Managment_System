using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Implementation.Requests;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Dto.DTOs.Requests.Financial;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Hr_ManagmentSystem_Test
{
    public class FinancialRequestServiceTests
    {
        private FinancialRequestService CreateService(
            Guid employeeId,
            out Mock<IGenericRepository<GenericRequest, int>> genericRepo,
            out Mock<IGenericRepository<FinancialRequest, int>> financialRepo,
            out Mock<IGenericRepository<RequestHistory, int>> historyRepo,
            out Mock<ICurrentUser> currentUserMock,
            out Mock<IRequestService> requestServiceMock
            )
        {
            genericRepo = new Mock<IGenericRepository<GenericRequest, int>>();
            financialRepo = new Mock<IGenericRepository<FinancialRequest, int>>();
            historyRepo = new Mock<IGenericRepository<RequestHistory, int>>();
            currentUserMock = new Mock<ICurrentUser>();
            requestServiceMock = new Mock<IRequestService>();

            currentUserMock.Setup(c => c.EmployeeId).Returns(employeeId);
            currentUserMock.Setup(c => c.Roles).Returns(new List<string>());

            var localizer = new Mock<IStringLocalizer<SharedResource>>();

            // _localizer["Some_Key"]
            localizer
                .Setup(l => l[It.IsAny<string>()])
                .Returns((string key) =>
                    new LocalizedString(key, key, resourceNotFound: false));

            // _localizer["Key {0}", arg0, arg1, ...]
            localizer
                .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string key, object[] args) =>
                {
                    var value = string.Format(key, args);
                    return new LocalizedString(key, value, resourceNotFound: false);
                });


            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateFinancialRequestDto, GenericRequest>();
                cfg.CreateMap<CreateFinancialRequestDto, FinancialRequest>();

                cfg.CreateMap<GenericRequest, GenericRequestListItemDto>();
                cfg.CreateMap<FinancialRequest, FinancialRequestDto>();
                cfg.CreateMap<RequestHistory, RequestHistoryDto>();
            });

            var mapper = mapperConfig.CreateMapper();

            genericRepo.Setup(r => r.AddAsync(It.IsAny<GenericRequest>())).Returns(Task.CompletedTask);
            financialRepo.Setup(r => r.AddAsync(It.IsAny<FinancialRequest>())).Returns(Task.CompletedTask);
            historyRepo.Setup(r => r.AddAsync(It.IsAny<RequestHistory>())).Returns(Task.CompletedTask);
            genericRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            return new FinancialRequestService(
               genericRepo.Object,
               financialRepo.Object,
               historyRepo.Object,
               currentUserMock.Object,
               NullLogger<FinancialRequestService>.Instance,
               localizer.Object,
               mapper,
               requestServiceMock.Object
            );


        }

        [Fact]
        public async Task CreateFinancalRequestAsync_WhenEmployeeIdEmpty_ReturnFail()
        {
            // Arrange
            var service = CreateService(
                Guid.Empty,
                out var genericRepo,
                out var financialRepo,
                out var historyRepo,
                out var currentUserMock,
                out var requestServiceMock
                );

            var dto = new CreateFinancialRequestDto { Amount = 1000m};

            // Act
            var result = await service.CreateFinancialRequestAsync( dto );

            //Assert
            
            Assert.False(result.Success );
            Assert.Equal("Auth_EmployeeNotLinked", result.Message);
                
        }

        [Fact]
        public async Task CreateFinancialRequestAsync_WhenAmountInvalid_ReturnFail()
        {
            // Arrange
            var service = CreateService(
                Guid.NewGuid(),
                out _, out _, out _, out _, out _
                );

            var dto = new CreateFinancialRequestDto { Amount = 0m };

            // Act
            var result  = await service.CreateFinancialRequestAsync(dto );


            Assert.False(result.Success );
            Assert.Equal("FinancialRequest_InvalidAmount", result.Message);
        }

        [Fact]
        public async Task CreateFinancialRequestAsync_WhenAmountIsNegative_ReturnFail()
        {
            // Arrange
            var service = CreateService(
                Guid.NewGuid(),
                out _, out _, out _, out _, out _
                );

            var dto = new CreateFinancialRequestDto { Amount = -1m };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);


            Assert.False(result.Success);
            Assert.Equal("FinancialRequest_InvalidAmount", result.Message);
        }


        [Fact]
        public async Task CreateFinancialRequestAsync_WhenDateRangeInvalid_ReturnFail()
        {
            // Arrange
            var service = CreateService(Guid.NewGuid(),
                out _, out _, out _, out _, out _);

            var dto = new CreateFinancialRequestDto
            {
                Amount = 10m,
                FromDate = DateTime.Today.AddDays(2),
                ToDate = DateTime.Today
            };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("FinancialRequest_InvalidDateRange", result.Message);
        }
        [Fact]
        public async Task CreateFinancialRequestAsync_WhenAmountIsNegative_ReturnFailInvalidAmount()
        {
            // Arrange
            var service = CreateService(Guid.NewGuid(), 
                out _, out _, out _, out _, out _);
            var dto = new CreateFinancialRequestDto
            {
                Amount = -5m,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                Currency = "JOD",
                FinancialType = FinancialType.Other
            };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("FinancialRequest_InvalidAmount", result.Message);
        }
        [Fact]
        public async Task CreateFinancialRequestAsync_WhenFromEqualsTo_IsValid()
        {
            // Arrange
            var service = CreateService(Guid.NewGuid(), out _, out _, out _ , out _ , out _);
            var d = DateTime.Today;

            var dto = new CreateFinancialRequestDto
            {
                Amount = 100m,
                FromDate = d,
                Currency = "JOD",
                FinancialType = FinancialType.Other,
                Details = "test"
            };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("FinancialRequest_Created", result.Message);
        }

        [Fact]
        public async Task CreateFinancialRequestAsync_WhenNoDatesProvided_IsValid()
        {
            // Arrange
            var service = CreateService(Guid.NewGuid(), out _, out _, out _ , out _ , out _);

            var dto = new CreateFinancialRequestDto
            {
                Amount = 100m,
                FromDate = null,
                ToDate = null,
                Currency = "JOD",
                FinancialType = FinancialType.Other,
                Details = "test"
            };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("FinancialRequest_Created", result.Message);
        }

        [Fact]
        public async Task CreateFinancialRequestAsync_WhenToDateOnly_IsValid()
        {
            // Arrange
            var service = CreateService(Guid.NewGuid(), out _, out _, out _, out _, out _);

            var dto = new CreateFinancialRequestDto
            {
                Amount = 100m,
                FromDate = null,
                ToDate = DateTime.Today,
                Currency = "JOD",
                FinancialType = FinancialType.Other,
                Details = "test"
            };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("FinancialRequest_Created", result.Message);
        }
        [Fact]
        public async Task CreateFinancialRequestAsync_WhenValidRange_IsValid()
        {
            // Arrange
            var service = CreateService(Guid.NewGuid(), out var genericRepo, out var finRepo, out var historyRepo , out _ , out _);

            var dto = new CreateFinancialRequestDto
            {
                Amount = 250m,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today.AddDays(3),
                Currency = "JOD",
                FinancialType = FinancialType.CourseFees,
                Details = "Course"
            };

            // Act
            var result = await service.CreateFinancialRequestAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("FinancialRequest_Created", result.Message);

            genericRepo.Verify(x => x.AddAsync(It.IsAny<GenericRequest>()), Times.Once);
            finRepo.Verify(x => x.AddAsync(It.IsAny<FinancialRequest>()), Times.Once);
            historyRepo.Verify(x => x.AddAsync(It.IsAny<RequestHistory>()), Times.Once);
            genericRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
