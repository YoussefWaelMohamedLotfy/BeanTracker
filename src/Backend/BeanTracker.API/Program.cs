using BeanTracker.API.Data;
using CommunityToolkit.Datasync.Server;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Database — Aspire-managed PostgreSQL connection
builder.AddNpgsqlDbContext<ApiDbContext>("beantracker-db");

// Datasync services
builder.Services.AddDatasyncServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize database schema and install PostgreSQL datasync triggers
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    await DatabaseInitializer.InitializeAsync(context).ConfigureAwait(false);
}

app.MapDefaultEndpoints();

app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
