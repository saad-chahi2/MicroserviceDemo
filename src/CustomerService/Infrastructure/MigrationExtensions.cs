using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure;

public static class MigrationExtensions
{
    /// <summary>
    /// Applique les migrations EF Core au démarrage, en réessayant tant que SQL Server n'est pas prêt
    /// (il met ~20-30 s à démarrer dans un conteneur).
    /// </summary>
    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app, int maxAttempts = 10)
        where TContext : DbContext
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = app.Services.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<TContext>();
                await context.Database.MigrateAsync();
                app.Logger.LogInformation("Migrations de la base appliquées");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                app.Logger.LogWarning(ex, "Base indisponible (tentative {Attempt}/{MaxAttempts}), nouvel essai dans 5 s", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
