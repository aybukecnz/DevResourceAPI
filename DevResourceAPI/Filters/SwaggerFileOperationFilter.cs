using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;

namespace DevResourceAPI
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var securityRequirement = new OpenApiSecurityRequirement();

            // --- DEĞİŞİKLİK BURADA BAŞLIYOR ---
            
            // İsteğin metodunu al (GET, POST, DELETE vs.)
            var httpMethod = context.ApiDescription.HttpMethod;

            // KURAL 1 GÜNCELLEMESİ: 
            // Eğer metod "GET" DEĞİLSE Api Key zorunluluğu ekle.
            // (GET ise bu if bloğuna girmez ve kilit koymaz)
            if (httpMethod?.ToUpper() != "GET")
            {
                securityRequirement.Add(new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                }, new List<string>());
            }
            
            // --- DEĞİŞİKLİK BURADA BİTİYOR ---


            // --- KURAL 2: TOKEN (BEARER) İŞLEMLERİ (Aynen kalıyor) ---
            var declaringTypeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>();
            var methodAttributes = context.MethodInfo.GetCustomAttributes(true);

            var hasAuthorize = declaringTypeAttributes.OfType<AuthorizeAttribute>().Any() ||
                               methodAttributes.OfType<AuthorizeAttribute>().Any();

            var allowAnonymous = methodAttributes.OfType<AllowAnonymousAttribute>().Any();

            // Eğer Authorize varsa VE AllowAnonymous yoksa Token kilidi ekle
            if (hasAuthorize && !allowAnonymous)
            {
                securityRequirement.Add(new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }, new List<string>());
            }

            // Eğer securityRequirement boş değilse operasyona ekle
            if (securityRequirement.Count > 0)
            {
                operation.Security = new List<OpenApiSecurityRequirement> { securityRequirement };
            }
        }
    }
}