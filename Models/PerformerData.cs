namespace SignMy2257.Models;

public class PerformerData
{
    public string? ProducerFormId { get; set; }
    public string? FullLegalName { get; set; }
    public string? Aliases { get; set; }
    public string? StageName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public List<SocialMediaAccount>? SocialMediaAccounts { get; set; } = new();
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class SocialMediaAccount
{
    public string? Platform { get; set; }
    public string? AccountName { get; set; }
    public string? Url { get; set; }
}
