using ASA.Core;
using ASA.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ASA.UnitTests.Core;

[TestFixture]
public class ContextProviderTests
{
    private Mock<ILogger<ContextProvider>> _loggerMock;
    private ContextProvider _contextProvider;
    private AsaExecutionContext _context;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ContextProvider>>();
        _contextProvider = new ContextProvider(_loggerMock.Object);
        _context = new AsaExecutionContext();
    }

    [Test]
    public void ResolveExpression_WithStepOutput_ReturnsCorrectValue()
    {
        // Arrange
        var stepOutput = new StepOutput { Data = "test value" };
        _context = new AsaExecutionContext
        {
            Steps = new Dictionary<string, StepOutput>
            {
                ["step1"] = stepOutput
            }
        };

        // Act
        var result = _contextProvider.ResolveExpression("steps.step1.data", _context);

        // Assert
        Assert.That(result, Is.EqualTo("test value"));
    }

    [Test]
    public void ResolveExpression_WithInvalidStep_ReturnsNull()
    {
        // Arrange
        _context = new AsaExecutionContext
        {
            Steps = new Dictionary<string, StepOutput>()
        };

        // Act
        var result = _contextProvider.ResolveExpression("steps.invalid.output", _context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ResolveExpression_WithTemplateString_ResolvesCorrectly()
    {
        // Arrange
        var context = new Dictionary<string, object>
        {
            { "name", "John" }
        };

        // Act
        var result = _contextProvider.ResolveExpression("Hello {name}", context);

        // Assert
        Assert.That(result, Is.EqualTo("Hello John"));
    }
}
