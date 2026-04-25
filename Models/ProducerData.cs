namespace SignMy2257.Models;

public class ProducerData
{
    public string? FullLegalName { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? ProductionName { get; set; }
    public DateTime? DateOfProduction { get; set; }
    public string? LocationOfProduction { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
