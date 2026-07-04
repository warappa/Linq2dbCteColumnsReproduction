using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<DataService>();

builder.Services.AddDbContext<ReadModelDbContext>(options =>
{
    options.UseSqlServer("Server=127.0.0.1,1433;Database=appdb;User Id=sa;Password=SQL-password12!!;TrustServerCertificate=True;", options =>
    {
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
    options.UseLazyLoadingProxies();
    options.UseLinqToDB();
});

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/health", () => Results.Ok("ok"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running. Navigate to /test to reproduce the issue.");

app.MapGet("/test", (DataService service) =>
{
    return service.QueryWithDependants(x => true, x => x.Name, false, null, null, 0, 20);
});

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReadModelDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

app.Run();
