namespace IdentityBase.WebApi
{
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.PlatformAbstractions;
    using Swashbuckle.AspNetCore.Swagger;

    public static class StartupSwagger
    {
        public static void AddDeveloperDocumentation(
            this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "IdentityBase API",
                    Version = "v1",
                });

                string filePath = Path.Combine(PlatformServices.Default
                    .Application.ApplicationBasePath,
                    "IdentityBase.WebApi.xml");

                if (File.Exists(filePath))
                {
                    c.IncludeXmlComments(filePath);
                }

                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();
                c.DescribeAllParametersInCamelCase();
            });
        }

        public static void UseDeveloperDocumentation(
            this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json",
                    "IdentityBase.WebApi");
            });
        }
    }
}