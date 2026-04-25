using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SignMy2257.Models;

namespace SignMy2257.Services;

public interface IPdfGenerationService
{
    Task<(byte[] PdfBytes, string FileName)> GenerateForm2257Async(ProducerData producer, PerformerData performer, List<string> imagePaths);
}

public class PdfGenerationService : IPdfGenerationService
{
    private readonly ILogger<PdfGenerationService> _logger;
    private readonly IFormStorageService _storageService;

    public PdfGenerationService(ILogger<PdfGenerationService> logger, IFormStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    public async Task<(byte[] PdfBytes, string FileName)> GenerateForm2257Async(ProducerData producer, PerformerData performer, List<string> imagePaths)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{performer?.Address}_{timestamp}.pdf";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(0.5f, QuestPDF.Infrastructure.Unit.Inch);
                
                page.Header().ShowOnce().Column(column =>
                {
                    column.Item().Text("FBI FORM 2257")
                        .Bold()
                        .FontSize(16)
                        .AlignCenter();
                    
                    column.Item().PaddingVertical(0.1f, Unit.Inch).Text("STATEMENT REGARDING THE USE OF MODELS IN SEXUALLY EXPLICIT CONDUCT")
                        .FontSize(11)
                        .AlignCenter();
                });

                page.Content().Column(column =>
                {
                    // Producer Information Section
                    column.Item().Section("PRODUCER INFORMATION", producer);

                    // Performer Information Section
                    column.Item().Section("PERFORMER INFORMATION", performer);

                    // Legal Statement
                    column.Item().LegalStatement();

                    // Images Section
                    if (imagePaths.Any())
                    {
                        column.Item().ImagesSection(imagePaths);
                    }

                    // Metadata
                    column.Item().Metadata(performer);
                });

                page.Footer().ShowOnce().Row(row =>
                {
                    row.RelativeColumn().Text(text =>
                    {
                        text.Span("Generated on ").FontSize(8);
                        text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")).FontSize(8).Bold();
                    });
                    
                    row.RelativeColumn().AlignRight().Text("Page ")
                        .FontSize(8);
                });
            });
        });

        byte[] pdfBytes = document.GeneratePdf();
        
        _logger.LogInformation("PDF generated for performer: {Performer}, Size: {Size} bytes", 
            performer?.FullLegalName, pdfBytes.Length);

        return (pdfBytes, fileName);
    }
}

public static class PdfExtensions
{
    public static void Section(this IContainer container, string title, object? data)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(0.1f, Unit.Inch).Text(title)
                .Bold()
                .FontSize(12);
            
            column.Item().PaddingVertical(0.05f, Unit.Inch).Column(inner =>
            {
                if (data is ProducerData producer)
                {
                    inner.Item().FormField("Full Legal Name", producer.FullLegalName);
                    inner.Item().FormField("Company Name", producer.CompanyName);
                    inner.Item().FormField("Address", producer.Address);
                    inner.Item().FormField("Production Name", producer.ProductionName);
                    inner.Item().FormField("Date of Production", producer.DateOfProduction?.ToString("yyyy-MM-dd"));
                    inner.Item().FormField("Location of Production", producer.LocationOfProduction);
                }
                else if (data is PerformerData performer)
                {
                    inner.Item().FormField("Full Legal Name", performer.FullLegalName);
                    inner.Item().FormField("Aliases", performer.Aliases);
                    inner.Item().FormField("Stage Name", performer.StageName);
                    inner.Item().FormField("Date of Birth", performer.DateOfBirth?.ToString("yyyy-MM-dd"));
                    inner.Item().FormField("Address", performer.Address);
                    
                    if (performer.SocialMediaAccounts?.Any() == true)
                    {
                        inner.Item().PaddingTop(0.05f, Unit.Inch).Text("Social Media Accounts").Bold().FontSize(9);
                        foreach (var account in performer.SocialMediaAccounts)
                        {
                            inner.Item().Text($"{account.Platform}: {account.AccountName} - {account.Url}")
                                .FontSize(8);
                        }
                    }
                }
            });
        });
    }

    public static void FormField(this IContainer container, string label, string? value)
    {
        container.Row(row =>
        {
            row.RelativeColumn(2).Text(label + ":").Bold().FontSize(9);
            row.RelativeColumn(3).Text(value ?? "_______________").FontSize(9);
        });
    }

    public static void LegalStatement(this IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(0.1f, Unit.Inch).Text("STATUTORY STATEMENT")
                .Bold()
                .FontSize(12);
            
            column.Item().PaddingVertical(0.05f, Unit.Inch).Column(inner =>
            {
                inner.Item().Text(
                    "I attest that I am familiar with 18 U.S.C. § 2257 and the regulations issued by the Department of Justice concerning " +
                    "the production and distribution of sexually explicit material. I attest that the individuals who appear in this material " +
                    "are of legal age as verified by the records I have maintained and which are available for inspection by the Department of Justice. " +
                    "I certify that I am not a sex offender as defined in 42 U.S.C. § 16911."
                ).FontSize(8);
            });
        });
    }

    public static void ImagesSection(this IContainer container, List<string> imagePaths)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(0.1f, Unit.Inch).Text("SUPPORTING DOCUMENTS")
                .Bold()
                .FontSize(12);
            
            column.Item().PaddingVertical(0.05f, Unit.Inch).Text("The following documents have been submitted with this form:")
                .FontSize(9);

            foreach (var imagePath in imagePaths)
            {
                if (File.Exists(imagePath))
                {
                    column.Item().Text($"✓ {Path.GetFileName(imagePath)}")
                        .FontSize(8);
                }
            }
        });
    }

    public static void Metadata(this IContainer container, PerformerData? performer)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(0.1f, Unit.Inch).Text("FORM METADATA")
                .Bold()
                .FontSize(10);
            
            column.Item().Row(row =>
            {
                row.RelativeColumn().Text("Submitted:").FontSize(7);
                row.RelativeColumn().Text(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")).FontSize(7);
            });
        });
    }
}
