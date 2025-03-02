using ASA.Core;
using ASA.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ASA.UnitTests.Core;

public class StepExecutorTests
{
    private Mock<IModuleRegistry> _moduleRegistryMock;
    private Mock<IContextProvider> _contextProviderMock;
    private Mock<ILogger<StepExecutor>> _loggerMock;
    private StepExecutor _executor;
    private AsaExecutionContext _context;

    [SetUp]
    public void Setup()
    {
        _moduleRegistryMock = new Mock<IModuleRegistry>();
        _contextProviderMock = new Mock<IContextProvider>();
        _loggerMock = new Mock<ILogger<StepExecutor>>();
        _executor = new StepExecutor(_moduleRegistryMock.Object, _contextProviderMock.Object, _loggerMock.Object);
        _context = new AsaExecutionContext
        {
            Request = new DefaultHttpContext().Request,
            Response = new DefaultHttpContext().Response,
            Steps = new Dictionary<string, StepOutput>()
        };
    }

    [Test]
    public async Task ExecuteAsync_WithValidSteps_ExecutesAll()
    {
        // Arrange
        var moduleMock = new Mock<IModule>();
        moduleMock.Setup(m => m.ExecuteAsync(
            It.IsAny<Dictionary<string, object>>(), 
            It.IsAny<AsaExecutionContext>()))
            .ReturnsAsync(new StepOutput { Success = true, Data = "test" });

        _moduleRegistryMock.Setup(r => r.GetModule("test-module"))
            .Returns(moduleMock.Object);

        var steps = new List<StepSpec>
        {
            new() { Name = "step1", Uses = "test-module" },
            new() { Name = "step2", Uses = "test-module" }
        };

        // Act
        await _executor.ExecuteAsync(steps, _context);

        // Assert
        moduleMock.Verify(m => m.ExecuteAsync(
            It.IsAny<Dictionary<string, object>>(), 
            It.IsAny<AsaExecutionContext>()), 
            Times.Exactly(2));
    }

    [Test]
    public async Task ExecuteAsync_WithFailedStep_StopsExecution()
    {
        // Arrange
        var moduleMock = new Mock<IModule>();
        moduleMock.Setup(m => m.ExecuteAsync(
            It.IsAny<Dictionary<string, object>>(), 
            It.IsAny<AsaExecutionContext>()))
            .ReturnsAsync(new StepOutput { Success = false, Error = "Test error" });

        _moduleRegistryMock.Setup(r => r.GetModule("test-module"))
            .Returns(moduleMock.Object);

        var steps = new List<StepSpec>
        {
            new() { Name = "step1", Uses = "test-module" },
            new() { Name = "step2", Uses = "test-module" }
        };

        // Act
        await _executor.ExecuteAsync(steps, _context);

        // Assert
        moduleMock.Verify(m => m.ExecuteAsync(
            It.IsAny<Dictionary<string, object>>(), 
            It.IsAny<AsaExecutionContext>()), 
            Times.Once);
        Assert.That(_context.Response.StatusCode, Is.EqualTo(500));
    }
}
