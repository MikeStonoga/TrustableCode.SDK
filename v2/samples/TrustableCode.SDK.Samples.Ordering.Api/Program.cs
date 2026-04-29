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
    options.UseInMemoryDatabase("ordering-sample"));

builder.Services.AddScoped<IOrderSnapshotStore, EfOrderSnapshotStore>();
builder.Services.AddScoped<IOrderingOutbox, EfOrderingOutbox>();
builder.Services.AddScoped<IBusinessEvidenceSink, EfBusinessEvidenceSink>();
builder.Services.AddScoped<IOrderingUnitOfWork, EfOrderingUnitOfWork>();
builder.Services.AddScoped<ISideEffectLifecycleStore, EfSideEffectLifecycleStore>();
builder.Services.AddScoped<OrderingApplicationService>();
builder.Services.AddScoped<PersistedOrderingApplicationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "TrustableCode Ordering API";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API v1");
});

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();

public partial class Program
{
}
