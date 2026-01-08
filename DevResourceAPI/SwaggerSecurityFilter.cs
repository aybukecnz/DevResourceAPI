using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace DevResourceAPI
{
    public class SwaggerSecurityFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 1. GÜVENLİ ATTRIBUTE OKUMA (Önceki düzeltme)
            var declaringTypeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>();
            var methodAttributes = context.MethodInfo.GetCustomAttributes(true);

            // 2. KONTROLLER
            var hasAuthorize = declaringTypeAttributes.OfType<AuthorizeAttribute>().Any() ||
                               methodAttributes.OfType<AuthorizeAttribute>().Any();

            var hasApiKey = declaringTypeAttributes.Any(attr => attr.GetType().Name == "ApiKeyAttribute") ||
                            methodAttributes.Any(attr => attr.GetType().Name == "ApiKeyAttribute");

            // 3. KİLİTLERİ EKLEME 
            if (hasAuthorize)
            {
                // HATA ÇÖZÜMÜ: Eğer liste null ise önce oluştur, sonra ekle.
                operation.Security ??= new List<OpenApiSecurityRequirement>();
                
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] { }
                    }
                });
            }

            if (hasApiKey)
            {
                // HATA ÇÖZÜMÜ: ApiKey için de aynısını yapıyoruz.
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                        },
                        new string[] { }
                    }
                });
            }
        }
    }
}