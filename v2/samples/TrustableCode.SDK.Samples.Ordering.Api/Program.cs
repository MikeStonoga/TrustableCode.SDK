using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrustableCode Ordering API Sample",
        Version = "v1",
        Description = "Realistic sample showing TrustableCode SDK admissions, governed transitions, evidence, outbox, and EF Core persistence."
    });
});
builder.Services.AddDbContext<OrderingDbContext>(options =>
{
    var provider = builder.Configuration["OrderingDatabase:Provider"] ?? "Sqlite";
    if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("ordering-sample");
        return;
    }

    var connectionString = builder.Configuration.GetConnectionString("OrderingDatabase")
        ?? builder.Configuration["OrderingDatabase:ConnectionString"]
        ?? "Data Source=ordering-sample.db";

    EnsureSqliteDirectoryExists(connectionString);
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IOrderSnapshotStore, EfOrderSnapshotStore>();
builder.Services.AddScoped<IOrderingOutbox, EfOrderingOutbox>();
builder.Services.AddScoped<IBusinessEvidenceSink, EfBusinessEvidenceSink>();
builder.Services.AddScoped<IOrderingUnitOfWork, EfOrderingUnitOfWork>();
builder.Services.AddScoped<ISideEffectLifecycleStore, EfSideEffectLifecycleStore>();
builder.Services.AddScoped<OrderingApplicationService>();
builder.Services.AddScoped<PersistedOrderingApplicationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "TrustableCode Ordering API";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API v1");
});

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();

static void EnsureSqliteDirectoryExists(string connectionString)
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

public partial class Program
{
}
