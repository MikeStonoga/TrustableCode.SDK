using TrustableCode.SDK.Samples.Ordering.Api.Persistence;

namespace TrustableCode.SDK.Samples.Ordering.Api.Configuration;

public static class OrderingApiApplicationBuilderExtensions
{
    public static WebApplication InitializeOrderingDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        db.Database.EnsureCreated();

        return app;
    }

    public static WebApplication UseOrderingApiSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "TrustableCode Ordering API";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API v1");
        });

        return app;
    }
}
