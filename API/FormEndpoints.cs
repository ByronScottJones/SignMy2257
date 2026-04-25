using SignMy2257.Models;
using SignMy2257.Services;

namespace SignMy2257.API;

public static class FormEndpoints
{
    public static void MapFormEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/forms");

        group.MapPost("/producer", CreateProducerForm)
            .WithName("CreateProducerForm")
            .WithDescription("Create a producer form and get a link for performers");

        group.MapGet("/producer/{formId}", GetProducerForm)
            .WithName("GetProducerForm")
            .WithDescription("Retrieve producer form data by ID");

        group.MapPost("/performer/{formId}", SubmitPerformerForm)
            .WithName("SubmitPerformerForm")
            .WithDescription("Submit completed performer form with images");

        group.MapPost("/images/{formId}", UploadImage)
            .WithName("UploadImage")
            .WithDescription("Upload ID image for the form");

        group.MapPost("/validate-complete/{formId}", ValidateAndComplete)
            .WithName("ValidateAndComplete")
            .WithDescription("Validate form completion and generate PDF");

        group.MapGet("/form/{formId}", GetFormData)
            .WithName("GetFormData")
            .WithDescription("Get current form data");
    }

    private static async Task<IResult> CreateProducerForm(
        ProducerFormRequest request,
        IFormStorageService storageService,
        IFormValidationService validationService,
        IStatisticsService statisticsService,
        ILogger<Program> logger)
    {
        var validation = validationService.ValidateProducerForm(request);
        if (!validation.IsValid)
            return Results.BadRequest(validation);

        try
        {
            var formId = await storageService.SaveProducerFormAsync(request);
            statisticsService.RecordFormCreated();

            var baseUrl = $"{GetBaseUrl(logger)}";
            var formUrl = $"{baseUrl}/form?id={formId}";

            logger.LogInformation("Producer form created: {FormId}", formId);

            return Results.Ok(new ProducerFormResponse
            {
                FormId = formId,
                FormUrl = formUrl,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating producer form");
            return Results.StatusCode(500);
        }
    }

    private static async Task<IResult> GetProducerForm(
        string formId,
        IFormStorageService storageService,
        ILogger<Program> logger)
    {
        try
        {
            var producerData = await storageService.LoadProducerFormAsync(formId);
            if (producerData == null)
                return Results.NotFound();

            return Results.Ok(producerData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving producer form: {FormId}", formId);
            return Results.StatusCode(500);
        }
    }

    private static async Task<IResult> UploadImage(
        string formId,
        IFormFile file,
        string imageType,
        IFormStorageService storageService,
        ILogger<Program> logger)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded");

        if (file.Length > 2 * 1024 * 1024) // 2MB limit
            return Results.BadRequest("File size exceeds 2MB limit");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            return Results.BadRequest("Only JPG and PNG files are allowed");

        try
        {
            using var stream = file.OpenReadStream();
            await storageService.SaveImageAsync(formId, imageType, stream, file.FileName);
            return Results.Ok(new { message = "Image uploaded successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading image for form: {FormId}", formId);
            return Results.StatusCode(500);
        }
    }

    private static async Task<IResult> SubmitPerformerForm(
        string formId,
        PerformerFormRequest request,
        IFormStorageService storageService,
        IFormValidationService validationService,
        ILogger<Program> logger)
    {
        var validation = validationService.ValidatePerformerForm(request);
        if (!validation.IsValid)
            return Results.BadRequest(validation);

        try
        {
            var performerData = new PerformerData
            {
                ProducerFormId = formId,
                FullLegalName = request.FullLegalName,
                Aliases = request.Aliases,
                StageName = request.StageName,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                SocialMediaAccounts = request.SocialMediaAccounts,
                SubmittedAt = DateTime.UtcNow
            };

            await storageService.SavePerformerFormAsync(formId, performerData);
            return Results.Ok(new { message = "Performer form submitted" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting performer form: {FormId}", formId);
            return Results.StatusCode(500);
        }
    }

    private static async Task<IResult> ValidateAndComplete(
        string formId,
        IFormStorageService storageService,
        IFormValidationService validationService,
        IPdfGenerationService pdfService,
        IStatisticsService statisticsService,
        ILogger<Program> logger)
    {
        try
        {
            var producerData = await storageService.LoadProducerFormAsync(formId);
            if (producerData == null)
                return Results.NotFound("Producer form not found");

            var performerPath = Path.Combine(await storageService.GetFormStoragePath(formId), "performer.json");
            if (!File.Exists(performerPath))
                return Results.BadRequest("Performer form not found");

            var imagePath = Path.Combine(await storageService.GetFormStoragePath(formId), "images");
            var requiredImages = new[] { "_IDFront.jpg", "_IDFront.png", "_IDBack.jpg", "_IDBack.png", "_IDFace.jpg", "_IDFace.png" };
            var imageFiles = Directory.Exists(imagePath) ? Directory.GetFiles(imagePath) : Array.Empty<string>();

            var hasIdFront = imageFiles.Any(f => f.Contains("_IDFront"));
            var hasIdBack = imageFiles.Any(f => f.Contains("_IDBack"));
            var hasIdFace = imageFiles.Any(f => f.Contains("_IDFace"));

            if (!hasIdFront || !hasIdBack || !hasIdFace)
                return Results.BadRequest(new { error = "All three ID images are required" });

            // Read performer data
            var performerJson = await File.ReadAllTextAsync(performerPath);
            var performerData = System.Text.Json.JsonSerializer.Deserialize<PerformerData>(performerJson);

            if (performerData == null)
                return Results.BadRequest("Invalid performer data");

            var imagePaths = imageFiles.ToList();
            var (pdfBytes, fileName) = await pdfService.GenerateForm2257Async(producerData, performerData, imagePaths);

            // Save PDF to storage
            var pdfPath = Path.Combine(await storageService.GetFormStoragePath(formId), fileName);
            await File.WriteAllBytesAsync(pdfPath, pdfBytes);

            statisticsService.RecordFormCompleted();
            logger.LogInformation("Form completed and PDF generated: {FormId}", formId);

            return Results.File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating and completing form: {FormId}", formId);
            return Results.StatusCode(500);
        }
    }

    private static async Task<IResult> GetFormData(
        string formId,
        IFormStorageService storageService)
    {
        var formPath = await storageService.GetFormStoragePath(formId);
        var producerFile = Path.Combine(formPath, "producer.json");
        var performerFile = Path.Combine(formPath, "performer.json");

        var response = new Dictionary<string, string>();

        if (File.Exists(producerFile))
            response["producer"] = await File.ReadAllTextAsync(producerFile);

        if (File.Exists(performerFile))
            response["performer"] = await File.ReadAllTextAsync(performerFile);

        return Results.Ok(response);
    }

    private static string GetBaseUrl(ILogger<Program> logger)
    {
        // In a real app, you'd get this from HttpContext or configuration
        return "http://localhost:5000";
    }
}
