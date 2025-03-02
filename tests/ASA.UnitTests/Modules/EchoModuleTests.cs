using ASA.Core.Models;
using ASA.Modules;
using NUnit.Framework;

namespace ASA.UnitTests.Modules;

public class EchoModuleTests
{
    private EchoModule _module;
    private AsaExecutionContext _context;

    [SetUp]
    public void Setup()
    {
        _module = new EchoModule();
        _context = new AsaExecutionContext();
    }

    [Test]
    public async Task ExecuteAsync_WithoutMessage_ReturnsDefaultMessage()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act
        var result = await _module.ExecuteAsync(parameters, _context);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.EqualTo("Hello, World!"));
    }

    [Test]
    public async Task ExecuteAsync_WithMessage_ReturnsMessage()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["message"] = "Test Message"
        };

        // Act
        var result = await _module.ExecuteAsync(parameters, _context);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data.ToString(), Is.EqualTo("Test Message"));
    }

    [Test]
    public async Task ExecuteAsync_WithNullMessage_ReturnsDefaultMessage()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["message"] = null
        };

        // Act
        var result = await _module.ExecuteAsync(parameters, _context);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.ToString(), Is.EqualTo("Hello, World!"));
    }
}
