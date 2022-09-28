using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GK.WebLib.Swagger
{
    public class SecureEndpointJwtAuthRequirementFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!context.ApiDescription
                .ActionDescriptor
                .EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any())
            {
                return;
            }

            var filter = new SecurityRequirementsOperationFilter(securitySchemaName: JwtBearerDefaults.AuthenticationScheme);
            filter.Apply(operation, context);
        }
    }
}
