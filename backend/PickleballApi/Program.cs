using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PickleballApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    });
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",  // Development
            "https://polite-tree-07cef7510.2.azurestaticapps.net"  // Production
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IGameService, GameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(Options =>
{
    Options.SwaggerEndpoint("/openapi/v1.json", "swaggerVersion");
});
    // app.UseCors("Development");
}

app.UseCors();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();


