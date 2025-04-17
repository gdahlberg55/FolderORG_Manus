using System;
using System.IO;
using System.Threading.Tasks;
using FolderORG.Manus.Core.Models;
using FolderORG.Manus.Domain.Rules.Services;
using Xunit;

namespace FolderORG.Manus.Tests.Rules
{
    public class PathValidationServiceTests
    {
        private readonly IPathValidationService _pathValidationService;

        public PathValidationServiceTests()
        {
            _pathValidationService = new PathValidationService();
        }

        [Fact]
        public async Task ValidatePathAsync_EmptyPath_ReturnsInvalid()
        {
            // Arrange
            string path = "";

            // Act
            var result = await _pathValidationService.ValidatePathAsync(path);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Path cannot be empty", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidatePathAsync_NullPath_ReturnsInvalid()
        {
            // Arrange
            string path = null;

            // Act
            var result = await _pathValidationService.ValidatePathAsync(path);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Path cannot be empty", result.ErrorMessage);
        }

        [Fact]
        public async Task ResolveVariablesAsync_EnvironmentVariable_ResolvesCorrectly()
        {
            // Arrange
            string envVarName = "TEST_VARIABLE";
            string envVarValue = "TestValue";
            Environment.SetEnvironmentVariable(envVarName, envVarValue);
            string path = $"C:\\Test\\%{envVarName}%\\File.txt";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path);

            // Assert
            Assert.Equal($"C:\\Test\\{envVarValue}\\File.txt", resolvedPath);

            // Cleanup
            Environment.SetEnvironmentVariable(envVarName, null);
        }

        [Fact]
        public async Task ResolveVariablesAsync_DateVariable_ResolvesCorrectly()
        {
            // Arrange
            string path = "C:\\Test\\${date:yyyy-MM-dd}\\File.txt";
            string expectedDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path);

            // Assert
            Assert.Equal($"C:\\Test\\{expectedDate}\\File.txt", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_UnknownVariable_LeavesUnchanged()
        {
            // Arrange
            string path = "C:\\Test\\${UNKNOWN_VARIABLE}\\File.txt";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path);

            // Assert
            Assert.Equal(path, resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_ContextVariables_ResolvesCorrectly()
        {
            // Arrange
            string variableName = "CustomVariable";
            string variableValue = "CustomValue";
            string path = $"C:\\Test\\${{{variableName}}}\\File.txt";
            var context = new PathValidationContext
            {
                Variables = { [variableName] = variableValue }
            };

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal($"C:\\Test\\{variableValue}\\File.txt", resolvedPath);
        }

        [Fact]
        public void NormalizePath_RelativePath_ResolvesToAbsolute()
        {
            // Arrange
            string relativePath = "Test\\File.txt";
            string expectedPath = Path.Combine(Environment.CurrentDirectory, relativePath);
            expectedPath = Path.GetFullPath(expectedPath);

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(relativePath);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);
        }

        [Fact]
        public void NormalizePath_PathWithDotsAndSlashes_NormalizesCorrectly()
        {
            // Arrange
            string path = "C:\\Test\\..\\Folder\\.\\.\\File.txt";
            string expectedPath = "C:\\Folder\\File.txt";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);
        }

        [Fact]
        public void RegisterVariableResolver_CustomResolver_ResolvesCorrectly()
        {
            // Arrange
            string variableName = "CustomResolver";
            string variableValue = "ResolvedValue";
            _pathValidationService.RegisterVariableResolver(variableName, _ => Task.FromResult(variableValue));
            string path = $"C:\\Test\\${{{variableName}}}\\File.txt";

            // Act
            string resolvedPath = _pathValidationService.ResolveVariablesAsync(path).GetAwaiter().GetResult();

            // Assert
            Assert.Equal($"C:\\Test\\{variableValue}\\File.txt", resolvedPath);
        }
    }
} 