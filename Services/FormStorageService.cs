using System.Text.Json;
using SignMy2257.Models;

namespace SignMy2257.Services;

public interface IFormStorageService
{
    Task<string> SaveProducerFormAsync(ProducerFormRequest request);
    Task<ProducerData?> LoadProducerFormAsync(string formId);
    Task SavePerformerFormAsync(string formId, PerformerData performerData);
    Task SaveImageAsync(string formId, string imageType, Stream imageStream, string fileName);
    Task<string> GetFormStoragePath(string formId);
    string GetStorageBasePath();
}

public class FormStorageService : IFormStorageService
{
    private readonly ILogger<FormStorageService> _logger;
    private readonly string _basePath;

    public FormStorageService(IConfiguration configuration, ILogger<FormStorageService> logger)
    {
        _logger = logger;
        _basePath = configuration["Storage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Data");
    }

    public string GetStorageBasePath() => _basePath;

    public async Task<string> SaveProducerFormAsync(ProducerFormRequest request)
    {
        var formId = Guid.NewGuid().ToString();
        var formPath = Path.Combine(_basePath, formId);
        
        Directory.CreateDirectory(formPath);

        var producerData = new ProducerData
        {
            FullLegalName = request.FullLegalName,
            CompanyName = request.CompanyName,
            Address = request.Address,
            ProductionName = request.ProductionName,
            DateOfProduction = request.DateOfProduction,
            LocationOfProduction = request.LocationOfProduction,
            CreatedAt = DateTime.UtcNow
        };

        var jsonPath = Path.Combine(formPath, "producer.json");
        var json = JsonSerializer.Serialize(producerData, new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(jsonPath, json);
        
        _logger.LogInformation("Producer form saved with ID: {FormId}", formId);
        
        return formId;
    }

    public async Task<ProducerData?> LoadProducerFormAsync(string formId)
    {
        var jsonPath = Path.Combine(_basePath, formId, "producer.json");
        
        if (!File.Exists(jsonPath))
        {
            _logger.LogWarning("Producer form not found: {FormId}", formId);
            return null;
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var producerData = JsonSerializer.Deserialize<ProducerData>(json);
        
        return producerData;
    }

    public async Task SavePerformerFormAsync(string formId, PerformerData performerData)
    {
        var formPath = Path.Combine(_basePath, formId);
        Directory.CreateDirectory(formPath);

        var jsonPath = Path.Combine(formPath, "performer.json");
        var json = JsonSerializer.Serialize(performerData, new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(jsonPath, json);
        
        _logger.LogInformation("Performer form saved for ID: {FormId}", formId);
    }

    public async Task SaveImageAsync(string formId, string imageType, Stream imageStream, string fileName)
    {
        var formPath = Path.Combine(_basePath, formId, "images");
        Directory.CreateDirectory(formPath);

        var imagePath = Path.Combine(formPath, $"{Path.GetFileNameWithoutExtension(fileName)}_{imageType}{Path.GetExtension(fileName)}");
        
        using (var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
        {
            await imageStream.CopyToAsync(fileStream);
        }

        _logger.LogInformation("Image saved: {ImagePath}", imagePath);
    }

    public Task<string> GetFormStoragePath(string formId)
    {
        return Task.FromResult(Path.Combine(_basePath, formId));
    }
}
