using System.Net;
using System.Text.Json;
using DevResourceAPI.Data; 
using DevResourceAPI.Models; 
using DevResourceAPI.Services; 
using Serilog; 

namespace DevResourceAPI.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    // Microsoft'un ILogger'ı artık arka planda Serilog kullanıyor (Program.cs sayesinde)
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
            // Trafiği normal akışına bırak
            await _next(context);
        }
        catch (Exception ex)
        {
            // ÖNCE DOSYAYA YAZ (Garanti Olsun)
            // Veritabanı çökse bile bu log dosyada (logs/log-.txt) duracak.
            _logger.LogError(ex, "Sistemde kritik bir hata oluştu! Mesaj: {Message}", ex.Message);

            // SONRA VERİTABANINA DENE (Dashboard İçin)
            await LogErrorToDatabase(context, ex);

            // KULLANICIYA CEVAP VER
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task LogErrorToDatabase(HttpContext context, Exception ex)
    {
        try
        {
            var dbContext = context.RequestServices.GetService<AppDbContext>();

            if (dbContext != null)
            {
                var errorLog = new ErrorLog
                {
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    // CreatedAt BaseEntity'den gelir
                };

                dbContext.ErrorLogs.Add(errorLog);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception dbEx)
        {
            // Eskiden: Console.WriteLine(...) yapıyordun, kaybolup gidiyordu.
            // Şimdi: Veritabanına yazamazsam, bunu da Serilog ile dosyaya "FATAL" olarak yazıyorum.
            
            Log.Fatal(dbEx, "Veritabanı Loglama Servisi ÇÖKTÜ! Asıl Hata Loglanamadı. Asıl Hata: {OriginalError}", ex.Message);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ServiceResult.Fail("Sunucu kaynaklı bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}