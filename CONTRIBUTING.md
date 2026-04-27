# Contributing to FBI Form 2257 System

Thank you for your interest in contributing to this project! This document provides guidelines and instructions for contributing.

## Code of Conduct

Please be respectful and professional in all interactions. We are committed to providing a welcoming and inclusive environment for all contributors.

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- Git
- Docker & Docker Compose (for containerized development)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:

   ```bash
   git clone https://github.com/yourusername/SignMy2257.git
   cd SignMy2257
   ```

3. Add upstream remote:

   ```bash
   git remote add upstream https://github.com/originalrepo/SignMy2257.git
   ```

## Development Workflow

### Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

Branch naming conventions:

- `feature/` for new features
- `fix/` for bug fixes
- `docs/` for documentation
- `test/` for tests
- `refactor/` for code refactoring

### Making Changes

1. Make your changes in the feature branch
2. Follow C# coding standards:
   - Use PascalCase for class names and public members
   - Use camelCase for local variables
   - Use async/await for I/O operations
   - Keep methods focused and relatively small
   - Add XML documentation comments for public APIs

3. Test your changes:

   ```bash
   dotnet build
   dotnet run
   ```

### Commit Messages

Write clear, descriptive commit messages:

```text
[feat] Add image cropping feature

- Add image editor component for ID photos
- Implement crop functionality in API
- Add validation for cropped dimensions
- Update documentation with new feature

Fixes #123
```

Format:

- Use imperative mood ("add" not "added")
- Limit first line to 50 characters
- Reference related issues with `Fixes #123` or `Closes #456`

### Push and Create Pull Request

1. Push to your fork:

   ```bash
   git push origin feature/your-feature-name
   ```

2. Create a Pull Request on GitHub with:
   - Clear title describing the change
   - Detailed description of what was changed and why
   - Reference to related issues
   - Screenshots if UI changes were made

## Code Standards

### C# Style Guide

```csharp
// Namespace declaration
namespace SignMy2257.Services;

// Using statements
using System.Collections.Generic;
using SignMy2257.Models;

// Class declaration with access modifiers
public class FormService
{
    private readonly ILogger<FormService> _logger;
    private readonly IConfiguration _configuration;

    // Constructor
    public FormService(ILogger<FormService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Process a form submission.
    /// </summary>
    /// <param name="form">The form data to process</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ProcessFormAsync(FormData form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        try
        {
            // Implementation
            _logger.LogInformation("Processing form {FormId}", form.Id);
            await SaveFormAsync(form);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing form {FormId}", form.Id);
            throw;
        }
    }
}
```

### Razor Components

```razor
@page "/form"
@using SignMy2257.Models
@inject IFormService FormService

<div class="container">
    @if (IsLoading)
    {
        <div class="spinner">Loading...</div>
    }
    else if (Form != null)
    {
        <h1>@Form.Title</h1>
        <!-- Component content -->
    }
</div>

@code {
    private FormData? Form;
    private bool IsLoading = true;

    protected override async Task OnInitializedAsync()
    {
        Form = await FormService.GetFormAsync();
        IsLoading = false;
    }
}
```

## Testing

### Unit Tests

Create tests for business logic in a `Tests` directory:

```csharp
[TestClass]
public class FormValidationServiceTests
{
    private FormValidationService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new FormValidationService();
    }

    [TestMethod]
    public void ValidateProducerForm_WithValidData_ReturnsValid()
    {
        // Arrange
        var request = new ProducerFormRequest
        {
            FullLegalName = "John Doe",
            CompanyName = "Test Company",
            // ... other required fields
        };

        // Act
        var result = _service.ValidateProducerForm(request);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }
}
```

### Integration Tests

Test API endpoints and service integration:

```csharp
[TestClass]
public class FormEndpointTests
{
    private WebApplicationFactory<Program> _factory;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TestMethod]
    public async Task CreateProducerForm_WithValidRequest_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var request = new ProducerFormRequest { /* ... */ };

        var response = await client.PostAsJsonAsync("/api/forms/producer", request);

        Assert.IsTrue(response.IsSuccessStatusCode);
    }
}
```

## Documentation

### Code Documentation

Add XML documentation comments for public APIs:

```csharp
/// <summary>
/// Validates a producer form submission.
/// </summary>
/// <param name="request">The producer form data to validate</param>
/// <returns>A validation response indicating success or failure</returns>
/// <exception cref="ArgumentNullException">Thrown if request is null</exception>
public FormValidationResponse ValidateProducerForm(ProducerFormRequest request)
{
    // Implementation
}
```

### README Updates

Update README.md if your changes:
- Add new API endpoints
- Change configuration options
- Add new features
- Fix documented issues

### Changelog

Update CHANGELOG.md with your changes following [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [Unreleased]

### Added
- Image cropping functionality for ID photos

### Fixed
- Rate limiting not resetting correctly

### Changed
- Updated QuestPDF to latest version
```

## Pull Request Process

1. Update relevant documentation
2. Ensure all tests pass
3. Verify no breaking changes
4. Provide a clear PR description
5. Link related issues

### PR Review Checklist

- [ ] Code follows style guidelines
- [ ] Changes are well-documented
- [ ] Tests are included and passing
- [ ] No new warnings introduced
- [ ] Commit messages are clear
- [ ] PR description is comprehensive

## Reporting Issues

When reporting bugs, include:

1. **Description**: Clear summary of the issue
2. **Steps to reproduce**: Detailed steps to recreate the problem
3. **Expected behavior**: What should happen
4. **Actual behavior**: What actually happens
5. **Environment**: OS, .NET version, Docker version if applicable
6. **Logs**: Relevant log output
7. **Screenshots**: For UI issues

Example:

```markdown
## Bug Report: PDF Generation Fails with Large Images

### Description
When uploading large ID images (near 2MB limit), PDF generation fails intermittently.

### Steps to Reproduce
1. Create producer form
2. Open performer form
3. Upload 2MB PNG image for ID front
4. Upload 2MB PNG image for ID back
5. Upload 2MB PNG image for ID with face
6. Click "Complete Form"

### Expected
PDF should generate and download

### Actual
Error: "System.OutOfMemoryException"

### Environment
- OS: macOS 14.0
- .NET: 9.0.0
- App Version: 1.0.0

### Logs
```
[ERROR] Error generating PDF - System.OutOfMemoryException
```

### Screenshots
[Include if applicable]
```

## Feature Requests

When suggesting features:

1. **Use case**: Describe the problem this solves
2. **Proposed solution**: How you envision it working
3. **Alternatives**: Other approaches considered
4. **Additional context**: Any other relevant information

## Release Process

Releases follow semantic versioning (MAJOR.MINOR.PATCH):

1. Update version in `SignMy2257.csproj`
2. Update CHANGELOG.md
3. Create git tag: `git tag -a v1.0.0 -m "Release version 1.0.0"`
4. Push tag: `git push origin v1.0.0`
5. GitHub Actions automatically builds and publishes release

## Questions?

- Open an issue with your question
- Reference relevant documentation
- Be as specific as possible

Thank you for contributing!
