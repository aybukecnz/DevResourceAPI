using System.Net;
using System.Text.Json;
using DevResourceAPI.Data; 
using DevResourceAPI.Models; 
using DevResourceAPI.Services; 


namespace DevResourceAPI.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Trafiği normal akışına bırak (Controller'a git)
            await _next(context);
        }
        catch (Exception ex)
        {
            // Hata olursa YAKALA 
            _logger.LogError(ex, "Sunucuda beklenmeyen bir hata oluştu.");
            // Hata Kayıt İşlemi 
            await LogErrorToDatabase(context, ex);
            // Kullanıcıya Cevap Verme İşlemi 
            await HandleExceptionAsync(context, ex);
        }
    }
    // Veritabanına Kayıt Yapan Metot
    private async Task LogErrorToDatabase(HttpContext context, Exception ex)
    {
        try
        {
            // Middleware içinden Scoped servislere (DbContext) erişmek için bu yöntemi kullanırız:
            var dbContext = context.RequestServices.GetService<AppDbContext>();

            if (dbContext != null)
            {
                var errorLog = new ErrorLog
                {
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    // CreatedAt BaseEntity sayesinde otomatik dolacak
                };

                dbContext.ErrorLogs.Add(errorLog);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"Hata loglanırken hata oluştu: {logEx.Message}");
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Hata kodu her zaman 500 (Internal Server Error) dönecek
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // "ServiceResult" yapısını kullan 
        var response = ServiceResult.Fail("Sunucu kaynaklı bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // camelCase (success, message) formatı için
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}