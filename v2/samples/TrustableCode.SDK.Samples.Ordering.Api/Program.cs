using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TrustableCode.SDK.Samples.Ordering;
using TrustableCode.SDK.Samples.Ordering.Api.Persistence;
using TrustableCode.SDK.TrustableModeling.Evidence;
using TrustableCode.SDK.TrustableModeling.SideEffects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseInMemoryDatabase("ordering-sample"));

builder.Services.AddScoped<IOrderSnapshotStore, EfOrderSnapshotStore>();
builder.Services.AddScoped<IOrderingOutbox, EfOrderingOutbox>();
builder.Services.AddScoped<IBusinessEvidenceSink, EfBusinessEvidenceSink>();
builder.Services.AddScoped<IOrderingUnitOfWork, EfOrderingUnitOfWork>();
builder.Services.AddSingleton<ISideEffectLifecycleStore, InMemorySideEffectLifecycleStore>();
builder.Services.AddScoped<OrderingApplicationService>();
builder.Services.AddScoped<PersistedOrderingApplicationService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program
{
}
