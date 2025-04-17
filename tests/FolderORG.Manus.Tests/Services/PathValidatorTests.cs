using FolderORG.Manus.Core.Models;
using FolderORG.Manus.Domain.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FolderORG.Manus.Tests.Services
{
    public class PathValidatorTests
    {
        private readonly PathValidator _validator;

        public PathValidatorTests()
        {
            _validator = new PathValidator();
        }

        [Fact]
        public void NormalizePath_RemovesRedundantSegments()
        {
            // Arrange
            var context = new PathValidationContext();
            string path = @"C:\test\..\folder\.\subfolder";

            // Act
            string result = _validator.NormalizePath(path, context);

            // Assert
            Assert.Equal(@"C:\folder\subfolder", result);
        }

        [Fact]
        public void NormalizePath_ConvertsForwardSlashesToBackslashes()
        {
            // Arrange
            var context = new PathValidationContext();
            string path = "C:/test/folder";

            // Act
            string result = _validator.NormalizePath(path, context);

            // Assert
            Assert.Equal(@"C:\test\folder", result);
        }

        [Fact]
        public void ResolveVariables_ResolvesEnvironmentVariables()
        {
            // Arrange
            var context = new PathValidationContext
            {
                ResolveEnvironmentVariables = true
            };
            
            string tempPath = Path.GetTempPath().TrimEnd('\\');
            string path = @"%TEMP%\test\file.txt";

            // Act
            string result = _validator.ResolveVariables(path, context);

            // Assert
            Assert.Equal($@"{tempPath}\test\file.txt", result);
        }

        [Fact]
        public void ResolveVariables_ResolvesCustomVariables()
        {
            // Arrange
            var context = new PathValidationContext();
            context.SetVariable("ProjectRoot", @"C:\Projects");
            context.SetVariable("ProjectName", "MyProject");
            
            string path = @"${ProjectRoot}\${ProjectName}\src\file.txt";

            // Act
            string result = _validator.ResolveVariables(path, context);

            // Assert
            Assert.Equal(@"C:\Projects\MyProject\src\file.txt", result);
        }

        [Fact]
        public async Task ValidatePath_InvalidCharacters_ReturnsError()
        {
            // Arrange
            var context = new PathValidationContext
            {
                CheckExistence = false
            };
            string path = @"C:\Test<>|file.txt";

            // Act
            var result = await _validator.ValidatePathAsync(path, context);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Issues, i => i.Code == "INVALID_PATH_CHARS");
        }

        [Fact]
        public async Task ValidatePath_PathTooLong_ReturnsError()
        {
            // Arrange
            var context = new PathValidationContext
            {
                CheckExistence = false,
                AllowLongPaths = false,
                MaxPathLength = 50
            };
            
            string path = @"C:\Test\AVeryLongPath\WithManySubfolders\AndAVeryLongFileName.txt";

            // Act
            var result = await _validator.ValidatePathAsync(path, context);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Issues, i => i.Code == "PATH_TOO_LONG");
        }

        [Fact]
        public async Task ValidatePath_ReservedDeviceName_ReturnsError()
        {
            // Arrange
            var context = new PathValidationContext
            {
                CheckExistence = false
            };
            string path = @"C:\Test\COM1\file.txt";

            // Act
            var result = await _validator.ValidatePathAsync(path, context);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Issues, i => i.Code == "RESERVED_NAME");
        }

        [Fact]
        public async Task ValidatePath_ValidPath_ReturnsSuccess()
        {
            // Arrange
            var context = new PathValidationContext
            {
                CheckExistence = false
            };
            string path = @"C:\Test\ValidFolder\file.txt";

            // Act
            var result = await _validator.ValidatePathAsync(path, context);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidatePath_ExpectFileButIsDirectory_ReturnsError()
        {
            // Arrange - use the temp directory which should exist on any system
            string tempDir = Path.GetTempPath().TrimEnd('\\', '/');
            
            var context = new PathValidationContext
            {
                ExpectFile = true,
                CheckExistence = true
            };

            // Act
            var result = await _validator.ValidatePathAsync(tempDir, context);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Issues, i => i.Code == "EXPECTED_FILE");
        }
    }
} 