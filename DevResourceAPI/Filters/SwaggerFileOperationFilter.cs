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

            var httpMethod = context.ApiDescription.HttpMethod;
            // --- KURAL 1: API KEY İŞLEMLERİ  ---
            // Eğer metod "GET" DEĞİLSE Api Key zorunluluğu ekle.
            // (GET ise bu if bloğuna girmez ve kilit koymaz)
            if (httpMethod?.ToUpper() != "GET")
            {
                securityRequirement.Add(new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                }, new List<string>());
            }
            
            // --- KURAL 2: TOKEN (BEARER) İŞLEMLERİ  ---
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

            if (securityRequirement.Count > 0)
            {
                operation.Security = new List<OpenApiSecurityRequirement> { securityRequirement };
            }
        }
    }
}