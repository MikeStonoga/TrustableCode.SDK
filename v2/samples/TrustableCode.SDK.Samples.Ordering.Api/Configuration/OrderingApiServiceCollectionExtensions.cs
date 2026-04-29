using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;

namespace TrustableCode.SDK.Samples.Ordering.Api.Configuration;

public static class OrderingApiServiceCollectionExtensions
{
    public static IServiceCollection AddOrderingApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        return services;
    }

    public static IServiceCollection AddOrderingApiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TrustableCode Ordering API Sample",
                Version = "v1",
                Description = "Realistic sample showing TrustableCode SDK admissions, governed transitions, evidence, outbox, and EF Core persistence."
            });
        });

        return services;
    }

    public static IServiceCollection AddOrderingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options =>
        {
            var provider = configuration["OrderingDatabase:Provider"] ?? "Sqlite";
            if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("ordering-sample");
                return;
            }

            var connectionString = configuration.GetConnectionString("OrderingDatabase")
                ?? configuration["OrderingDatabase:ConnectionString"]
                ?? "Data Source=ordering-sample.db";

            EnsureSqliteDirectoryExists(connectionString);
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IOrderSnapshotStore, EfOrderSnapshotStore>();
        services.AddScoped<IOrderingOutbox, EfOrderingOutbox>();
        services.AddScoped<IBusinessEvidenceSink, EfBusinessEvidenceSink>();
        services.AddScoped<IOrderingUnitOfWork, EfOrderingUnitOfWork>();
        services.AddScoped<ISideEffectLifecycleStore, EfSideEffectLifecycleStore>();
        services.AddScoped<OrderingApplicationService>();
        services.AddScoped<PersistedOrderingApplicationService>();
        services.AddScoped<OrderingQueryService>();

        return services;
    }

    private static void EnsureSqliteDirectoryExists(string connectionString)
    {
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource)
            || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
