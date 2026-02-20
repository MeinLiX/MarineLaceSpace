using MarineLaceSpace.Interfaces.Repositories.Auth;

namespace Auth.WebHost.Services;

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                await refreshTokenRepository.DeleteExpiredTokensAsync();
                _logger.LogInformation("Expired refresh tokens cleaned up successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired refresh tokens");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
