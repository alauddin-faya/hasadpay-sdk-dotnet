using HasadPay.Net.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Register HasadPay with Dependency Injection from appsettings.json
builder.Services.AddHasadPay(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
