namespace DevResourceAPI.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string APIKEYNAME = "x-api-key"; // Header'da aranacak anahtar ismi

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (context.Request.Method == "GET")
        {
            await _next(context);
            return; 
        }
        // 1. İstekte "x-api-key" başlığı var mı?
        if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key bulunamadi! (x-api-key eksik)");
            return;
        }

        // 2. appsettings.json'daki gerçek şifreyi al
        var apiKey = configuration.GetValue<string>("ApiKey");

        // 3. Şifreler eşleşiyor mu?
        if (!apiKey!.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Gecersiz API Key!");
            return;
        }

        await _next(context);
    }
}