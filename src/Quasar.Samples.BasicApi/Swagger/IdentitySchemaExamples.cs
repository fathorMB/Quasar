using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Quasar.Identity.Web;

namespace Quasar.Samples.BasicApi.Swagger;

/// <summary>
/// Provides default examples for Swagger UI to match the seeded demo credentials.
/// </summary>
internal sealed class IdentitySchemaExamples : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(LoginDto))
        {
            schema.Example = new OpenApiObject
            {
                ["username"] = new OpenApiString("swagger-demo"),
                ["password"] = new OpenApiString("Passw0rd!")
            };
        }
        else if (context.Type == typeof(RegisterDto))
        {
            schema.Example = new OpenApiObject
            {
                ["username"] = new OpenApiString("swagger-demo"),
                ["email"] = new OpenApiString("swagger-demo@example.com"),
                ["password"] = new OpenApiString("Passw0rd!")
            };
        }
    }
}
