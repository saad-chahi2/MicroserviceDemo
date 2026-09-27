using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Infrastructure;
using OrderService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (injection de dépendances) ----------
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La chaîne de connexion 'DefaultConnection' est manquante.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderAppService, OrderAppService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(tags: ["ready"]);

var app = builder.Build();

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "OrderService v1"));
}

// Pas de UseHttpsRedirection : le conteneur écoute en HTTP, le TLS sera géré par l'Ingress Kubernetes.
app.MapControllers();

// Utilisés par les probes Kubernetes :
// - live  : le process répond (sinon K8s redémarre le pod)
// - ready : la base est joignable (sinon K8s n'envoie plus de trafic au pod)
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await app.ApplyMigrationsAsync<AppDbContext>();
}

await app.RunAsync();
