using TrustableCode.SDK.Samples.Ordering.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrderingApiControllers();
builder.Services.AddOrderingApiSwagger();
builder.Services.AddOrderingInfrastructure(builder.Configuration);

var app = builder.Build();

app.InitializeOrderingDatabase();
app.UseOrderingApiSwagger();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();

public partial class Program
{
}
