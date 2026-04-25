using System.Collections.Concurrent;
using SignMy2257.Models;

namespace SignMy2257.Services;

public interface IFormValidationService
{
    FormValidationResponse ValidateProducerForm(ProducerFormRequest request);
    FormValidationResponse ValidatePerformerForm(PerformerFormRequest request);
    FormValidationResponse ValidateImages(List<string> imagePaths);
}

public interface IStatisticsService
{
    void RecordFormCreated();
    void RecordFormCompleted();
    StatisticsResponse GetStatistics();
}

public class FormValidationService : IFormValidationService
{
    public FormValidationResponse ValidateProducerForm(ProducerFormRequest request)
    {
        var response = new FormValidationResponse { IsValid = true };

        if (string.IsNullOrWhiteSpace(request.FullLegalName))
            response.Errors.Add("Producer's full legal name is required");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            response.Errors.Add("Company name is required");

        if (string.IsNullOrWhiteSpace(request.Address))
            response.Errors.Add("Address is required");

        if (string.IsNullOrWhiteSpace(request.ProductionName))
            response.Errors.Add("Production name is required");

        if (request.DateOfProduction == null)
            response.Errors.Add("Date of production is required");

        if (string.IsNullOrWhiteSpace(request.LocationOfProduction))
            response.Errors.Add("Location of production is required");

        response.IsValid = response.Errors.Count == 0;
        return response;
    }

    public FormValidationResponse ValidatePerformerForm(PerformerFormRequest request)
    {
        var response = new FormValidationResponse { IsValid = true };

        if (string.IsNullOrWhiteSpace(request.FullLegalName))
            response.Errors.Add("Performer's full legal name is required");

        if (string.IsNullOrWhiteSpace(request.StageName))
            response.Errors.Add("Stage name is required");

        if (request.DateOfBirth == null)
            response.Errors.Add("Date of birth is required");
        else if (request.DateOfBirth > DateTime.Now.AddYears(-18))
            response.Errors.Add("Performer must be at least 18 years old");

        if (string.IsNullOrWhiteSpace(request.Address))
            response.Errors.Add("Address is required");

        response.IsValid = response.Errors.Count == 0;
        return response;
    }

    public FormValidationResponse ValidateImages(List<string> imagePaths)
    {
        var response = new FormValidationResponse { IsValid = true };

        if (imagePaths?.Count < 3)
        {
            response.Errors.Add("All three identity documents (front, back, and face) are required");
            response.IsValid = false;
        }

        return response;
    }
}

public class StatisticsService : IStatisticsService
{
    private readonly IFormStorageService _storageService;
    private readonly ConcurrentDictionary<string, int> _hourlyRequests;
    private int _formsCreated;
    private int _formsCompleted;
    private DateTime _lastFormCreated;

    public StatisticsService(IFormStorageService storageService)
    {
        _storageService = storageService;
        _hourlyRequests = new ConcurrentDictionary<string, int>();
        LoadStatistics();
    }

    public void RecordFormCreated()
    {
        _formsCreated++;
        _lastFormCreated = DateTime.UtcNow;
        RecordHourlyRequest();
    }

    public void RecordFormCompleted()
    {
        _formsCompleted++;
        RecordHourlyRequest();
    }

    public StatisticsResponse GetStatistics()
    {
        var basePath = _storageService.GetStorageBasePath();
        var pendingForms = 0;

        if (Directory.Exists(basePath))
        {
            var dirs = Directory.GetDirectories(basePath);
            pendingForms = dirs.Count(d => !File.Exists(Path.Combine(d, "performer.json")));
        }

        return new StatisticsResponse
        {
            TotalFormsCreated = _formsCreated,
            CompletedForms = _formsCompleted,
            PendingForms = pendingForms,
            LastFormCreated = _lastFormCreated,
            RequestsPerHour = new Dictionary<string, int>(_hourlyRequests)
        };
    }

    private void RecordHourlyRequest()
    {
        var hour = DateTime.UtcNow.ToString("yyyy-MM-dd HH:00");
        _hourlyRequests.AddOrUpdate(hour, 1, (_, count) => count + 1);

        // Keep only last 24 hours
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var keysToRemove = _hourlyRequests.Keys
            .Where(k => DateTime.ParseExact(k, "yyyy-MM-dd HH:00", null) < cutoffTime)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _hourlyRequests.TryRemove(key, out _);
        }
    }

    private void LoadStatistics()
    {
        var basePath = _storageService.GetStorageBasePath();
        if (Directory.Exists(basePath))
        {
            var dirs = Directory.GetDirectories(basePath);
            _formsCreated = dirs.Length;
            _formsCompleted = dirs.Count(d => File.Exists(Path.Combine(d, "performer.json")));
        }
    }
}
