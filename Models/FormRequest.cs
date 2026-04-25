namespace SignMy2257.Models;

public class ProducerFormRequest
{
    public string? FullLegalName { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? ProductionName { get; set; }
    public DateTime? DateOfProduction { get; set; }
    public string? LocationOfProduction { get; set; }
}

public class PerformerFormRequest
{
    public string? FullLegalName { get; set; }
    public string? Aliases { get; set; }
    public string? StageName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public List<SocialMediaAccount>? SocialMediaAccounts { get; set; } = new();
}

public class ProducerFormResponse
{
    public string? FormId { get; set; }
    public string? FormUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ImageUploadRequest
{
    public string? FormId { get; set; }
    public string? ImageType { get; set; } // IDFront, IDBack, IDFace
    public IFormFile? File { get; set; }
}

public class FormValidationResponse
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class StatisticsResponse
{
    public int TotalFormsCreated { get; set; }
    public int CompletedForms { get; set; }
    public int PendingForms { get; set; }
    public DateTime LastFormCreated { get; set; }
    public Dictionary<string, int> RequestsPerHour { get; set; } = new();
}
