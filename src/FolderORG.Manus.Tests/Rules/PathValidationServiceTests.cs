using System;
using System.Collections.Generic;
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

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataName_ResolvesCorrectly()
        {
            // Arrange
            var fileMetadata = new FileMetadata
            {
                Name = "document.pdf"
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\Archive\\${file:name}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\Archive\\document.pdf\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataExtension_ResolvesCorrectly()
        {
            // Arrange
            var fileMetadata = new FileMetadata
            {
                Extension = ".pdf"
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\${file:extension}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\.pdf\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataNameWithoutExt_ResolvesCorrectly()
        {
            // Arrange
            var fileMetadata = new FileMetadata
            {
                Name = "document.pdf"
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\Archive\\${file:namewithoutext}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\Archive\\document\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataCreationDate_ResolvesCorrectly()
        {
            // Arrange
            DateTime creationDate = new DateTime(2023, 5, 15);
            var fileMetadata = new FileMetadata
            {
                CreationDate = creationDate
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\${file:creationdate}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\2023-05-15\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataCategory_ResolvesCorrectly()
        {
            // Arrange
            var fileMetadata = new FileMetadata
            {
                Category = "Documents"
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\${file:category}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\Documents\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataCustomProperty_ResolvesCorrectly()
        {
            // Arrange
            var fileMetadata = new FileMetadata
            {
                CustomProperties = new Dictionary<string, string>
                {
                    { "project", "ProjectX" }
                }
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\${file:project}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\ProjectX\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_FileMetadataUnknownProperty_LeavesUnchanged()
        {
            // Arrange
            var fileMetadata = new FileMetadata();
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata
            };
            
            string path = "C:\\${file:unknown}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal(path, resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_ExpressionSimpleConcatenation_ResolvesCorrectly()
        {
            // Arrange
            var context = new PathValidationContext
            {
                Variables = { ["prefix"] = "Pre-", ["suffix"] = "-Post" }
            };
            
            string path = "C:\\${expr:${prefix} + File + ${suffix}}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            Assert.Equal("C:\\Pre-File-Post\\", resolvedPath);
        }

        [Fact]
        public async Task ResolveVariablesAsync_ComplexPathWithMultipleVariables_ResolvesCorrectly()
        {
            // Arrange
            var fileMetadata = new FileMetadata
            {
                Category = "Documents",
                CreationDate = new DateTime(2023, 5, 15)
            };
            
            var context = new PathValidationContext
            {
                FileMetadata = fileMetadata,
                Variables = { ["username"] = "john.doe" }
            };
            
            string path = "C:\\Users\\${username}\\${file:category}\\${date:yyyy}\\${file:creationdate}\\";

            // Act
            string resolvedPath = await _pathValidationService.ResolveVariablesAsync(path, context);

            // Assert
            string currentYear = DateTime.Now.Year.ToString();
            Assert.Equal($"C:\\Users\\john.doe\\Documents\\{currentYear}\\2023-05-15\\", resolvedPath);
        }

        [Fact]
        public void NormalizePath_WithRedundantSeparators_RemovesRedundancy()
        {
            // Arrange
            string path = "C:\\Test\\\\Folder\\\\\\File.txt";
            string expectedPath = "C:\\Test\\Folder\\File.txt";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);
        }

        [Fact]
        public void NormalizePath_WithTrailingSeparator_RemovesTrailingSeparator()
        {
            // Arrange
            string path = "C:\\Test\\Folder\\";
            string expectedPath = "C:\\Test\\Folder";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);
        }

        [Fact]
        public void NormalizePath_WithTilde_ExpandsToUserProfile()
        {
            // Arrange
            string path = "~\\Documents\\File.txt";
            string expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                "Documents\\File.txt");
            expectedPath = Path.GetFullPath(expectedPath);

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);
        }

        [Fact]
        public void NormalizePath_WithEnvironmentVariable_ExpandsVariable()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TEST_PATH_VAR", "TestPathValue");
            string path = "%TEST_PATH_VAR%\\File.txt";
            string expectedPath = Path.Combine(
                Environment.CurrentDirectory,
                "TestPathValue\\File.txt");
            expectedPath = Path.GetFullPath(expectedPath);

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);

            // Cleanup
            Environment.SetEnvironmentVariable("TEST_PATH_VAR", null);
        }

        [Fact]
        public void NormalizePath_WithUNCPath_PreservesUNCFormat()
        {
            // Arrange
            string path = "\\\\server\\share\\folder\\file.txt";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.StartsWith("\\\\", normalizedPath);
            Assert.Contains("server\\share\\folder\\file.txt", normalizedPath);
        }

        [Fact]
        public void NormalizePath_WithMixedSeparators_StandardizesSeparators()
        {
            // Arrange
            string path = "C:/Test\\Folder/File.txt";
            string expectedPath = "C:\\Test\\Folder\\File.txt";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.Equal(expectedPath, normalizedPath);
        }

        [Fact]
        public void NormalizePath_WithLongPath_AddsLongPathPrefix()
        {
            // Arrange - Create a very long path that exceeds 260 characters
            string longSubdir = new string('a', 240);
            string path = $"C:\\{longSubdir}\\file.txt";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            Assert.StartsWith("\\\\?\\", normalizedPath);
            Assert.Contains(longSubdir, normalizedPath);
        }

        [Fact]
        public async Task ValidatePathAsync_WithDirectoryNesting_DetectsExcessiveNesting()
        {
            // Arrange - Don't try to use an actual deeply nested path, just mock the validation
            string path = "C:\\level1\\level2\\level3\\level4\\level5\\level6\\level7\\level8\\level9\\level10\\level11\\level12\\level13\\level14\\level15\\level16\\file.txt";
            var context = new PathValidationContext
            {
                CheckExistence = false // Disable existence checking for testing
            };

            // Mock the directory to appear to exist for our test
            // Normally we'd use a mocking framework, but for this simple case we can use a wrapper

            // Act
            var result = await _pathValidationService.ValidatePathAsync(path, context);

            // Assert
            Assert.Contains(result.ValidationIssues, issue => issue.Contains("excessive directory nesting"));
        }

        [Fact]
        public async Task ValidatePathAsync_WithRedundantSegments_DetectsRedundancy()
        {
            // Arrange
            string path = "C:\\Test\\Test\\Test\\Test\\Test\\File.txt";
            var context = new PathValidationContext
            {
                CheckExistence = false // Disable existence checking for testing
            };

            // Act
            var result = await _pathValidationService.ValidatePathAsync(path, context);

            // Assert
            Assert.Contains(result.ValidationIssues, issue => issue.Contains("redundant named segments"));
        }

        [Fact]
        public async Task ValidatePathAsync_WithContextDirectoryCreation_CreatesDirectory()
        {
            // Arrange
            string tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string path = Path.Combine(tempBase, "NewFolder", "NewSubfolder", "test.txt");
            var context = new PathValidationContext
            {
                CreateDirectories = true
            };

            try
            {
                // Act
                var result = await _pathValidationService.ValidatePathAsync(path, context);

                // Assert
                Assert.True(Directory.Exists(Path.GetDirectoryName(path)));
                Assert.Contains(result.ValidationIssues, issue => issue.Contains("Parent directory created"));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempBase))
                {
                    Directory.Delete(tempBase, true);
                }
            }
        }

        [Fact]
        public void NormalizePath_WithDriveLetter_LowercasesDriveLetter()
        {
            // Arrange
            string path = "C:\\Test\\File.txt";
            string expectedPath = "c:\\Test\\File.txt";

            // Act
            string normalizedPath = _pathValidationService.NormalizePath(path);

            // Assert
            // Extract just the drive letter for comparison
            Assert.Equal(expectedPath[0], normalizedPath[0]);
        }
    }
} 