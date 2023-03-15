using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace OpenApi.Library.Swagger;

using static Extensions.AssemblyMetadataInformation;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerEndpoint(this IServiceCollection services) =>
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = AssemblyName, Version = AssemblyVersion, Description = $"<p><strong>Build: </strong>{InformationalVersion}" });
            var securityScheme = new OpenApiSecurityScheme {
                Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme },
            };
            options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            options.OperationFilter<SecureEndpointJwtAuthRequirementFilter>();

        });

    public static IApplicationBuilder UseSwaggerEndpoint(this IApplicationBuilder app) =>
        app.UseSwagger()
           .UseSwaggerUI(options => options.SwaggerEndpoint($"/swagger/v1/swagger.json", $"{AssemblyName} {AssemblyVersion}"));
}
