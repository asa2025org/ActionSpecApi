using ASA.Core;
using ASA.Core.Models;
using ASA.Modules;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Text.Json;

namespace ASA.UnitTests.Modules
{
    public class ResponseFormatterModuleTests
    {
        private ResponseFormatterModule _module;
        private AsaExecutionContext _context;

        [SetUp]
        public void Setup()
        {
            _module = new ResponseFormatterModule();
            _context = new AsaExecutionContext
            {
                Response = new DefaultHttpContext().Response
            };
        }

        [Test]
        public async Task ExecuteAsync_WithDefaultParameters_SetsDefaultValues()
        {
            // Arrange
            var parameters = new Dictionary<string, object>();

            // Act
            var result = await _module.ExecuteAsync(parameters, _context);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(_context.Response.StatusCode, Is.EqualTo(200));
            Assert.That(_context.Response.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public async Task ExecuteAsync_WithCustomStatus_SetsCorrectStatusCode()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["statusCode"] = 201
            };

            // Act
            var result = await _module.ExecuteAsync(parameters, _context);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(_context.Response.StatusCode, Is.EqualTo(201));
        }

        [Test]
        public async Task ExecuteAsync_WithJsonBody_FormatsJsonCorrectly()
        {
            // Arrange
            var testObject = new { name = "test", value = 123 };
            var parameters = new Dictionary<string, object>
            {
                ["body"] = testObject
            };

            // Act
            var result = await _module.ExecuteAsync(parameters, _context);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(_context.Response.ContentType, Is.EqualTo("application/json"));
            Assert.That(result.Data.ToString(), Does.Contain("\"name\":\"test\""));
        }

        [Test]
        public async Task ExecuteAsync_WithPlainTextContentType_FormatsTextCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["contentType"] = "text/plain",
                ["body"] = "Hello World"
            };

            // Act
            var result = await _module.ExecuteAsync(parameters, _context);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(_context.Response.ContentType, Is.EqualTo("text/plain"));
            Assert.That(result.Data.ToString(), Is.EqualTo("{ status = 200, body = Hello World }"));
        }
    }
}
