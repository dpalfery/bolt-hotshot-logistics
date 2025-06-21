using HotshotLogistics.Application.Services;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Contracts.Repositories;
using HotshotLogistics.Domain.Models;
using Moq;

namespace HotshotLogistics.Tests;

public class DriverServiceTests
{
    [Fact]
    public async Task GetDriversAsync_ReturnsDriversFromRepository()
    {
        var expected = new List<IDriver> { new Driver { Id = 1 }, new Driver { Id = 2 } };
        var repoMock = new Mock<IDriverRepository>();
        repoMock.Setup(r => r.GetDriversAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var service = new DriverService(repoMock.Object);

        var result = await service.GetDriversAsync();

        Assert.Equal(expected, result);
        repoMock.Verify(r => r.GetDriversAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDriverByIdAsync_UsesRepository()
    {
        var driver = new Driver { Id = 1 };
        var repoMock = new Mock<IDriverRepository>();
        repoMock.Setup(r => r.GetDriverByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(driver);
        var service = new DriverService(repoMock.Object);

        var result = await service.GetDriverByIdAsync(1);

        Assert.Equal(driver, result);
        repoMock.Verify(r => r.GetDriverByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDriverAsync_CallsRepository()
    {
        var newDriver = new Driver { Id = 1 };
        var repoMock = new Mock<IDriverRepository>();
        repoMock.Setup(r => r.CreateDriverAsync(newDriver, It.IsAny<CancellationToken>())).ReturnsAsync(newDriver);
        var service = new DriverService(repoMock.Object);

        var result = await service.CreateDriverAsync(newDriver);

        Assert.Equal(newDriver, result);
        repoMock.Verify(r => r.CreateDriverAsync(newDriver, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class JobServiceTests
{
    [Fact]
    public async Task GetJobsAsync_ReturnsJobsFromRepository()
    {
        var expected = new List<IJob> { new Job { Id = "1" }, new Job { Id = "2" } };
        var repoMock = new Mock<IJobRepository>();
        repoMock.Setup(r => r.GetJobsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected); // Fix: Explicitly pass CancellationToken

        var service = new JobService(repoMock.Object);

        var result = await service.GetJobsAsync();

        Assert.Equal(expected, result);
        repoMock.Verify(r => r.GetJobsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetJobByIdAsync_UsesRepository()
    {
        var job = new Job { Id = "1" };
        var repoMock = new Mock<IJobRepository>();
        repoMock.Setup(r => r.GetJobByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(job); // Fix: Explicitly pass CancellationToken
        var service = new JobService(repoMock.Object);

        var result = await service.GetJobByIdAsync("1");

        Assert.Equal(job, result);
        repoMock.Verify(r => r.GetJobByIdAsync("1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateJobAsync_CallsRepository()
    {
        var job = new Job { Id = "1" };
        var repoMock = new Mock<IJobRepository>();
        repoMock.Setup(r => r.CreateJobAsync(job, It.IsAny<CancellationToken>())).ReturnsAsync(job);
        var service = new JobService(repoMock.Object);

        var result = await service.CreateJobAsync(job);

        Assert.Equal(job, result);
        repoMock.Verify(r => r.CreateJobAsync(job, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateJobAsync_CallsRepository()
    {
        var job = new Job { Id = "1" };
        var repoMock = new Mock<IJobRepository>();
        repoMock.Setup(r => r.UpdateJobAsync("1", job, It.IsAny<CancellationToken>())).ReturnsAsync(job);
        var service = new JobService(repoMock.Object);

        var result = await service.UpdateJobAsync("1", job);

        Assert.Equal(job, result);
        repoMock.Verify(r => r.UpdateJobAsync("1", job, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteJobAsync_CallsRepository()
    {
        var repoMock = new Mock<IJobRepository>();
        repoMock.Setup(r => r.DeleteJobAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new JobService(repoMock.Object);

        var result = await service.DeleteJobAsync("1", It.IsAny<CancellationToken>());

        Assert.True(result);
        repoMock.Verify(r => r.DeleteJobAsync("1", It.IsAny<CancellationToken>()), Times.Once);
    }
}

