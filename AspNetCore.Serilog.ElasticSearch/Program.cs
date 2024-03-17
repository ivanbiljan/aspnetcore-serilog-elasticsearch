using AspNetCore.Serilog.ElasticSearch.Handlers;
using AspNetCore.Serilog.ElasticSearch.Infrastructure.Behaviors;
using AspNetCore.Serilog.ElasticSearch.Infrastructure.Logging;
using AspNetCore.Serilog.ElasticSearch.Infrastructure.Persistence;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();
builder.ConfigureDatabase();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(
    configuration =>
    {
        configuration.RegisterServicesFromAssemblyContaining<Program>();

        configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated(); // Demo purposes
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogHttpLogging();

app.UseHttpsRedirection();

app.MapGet(
        "/weatherforecast",
        async (IMediator mediator, CancellationToken cancellationToken) =>
            await mediator.Send(new GetForecast.Query(), cancellationToken))
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();