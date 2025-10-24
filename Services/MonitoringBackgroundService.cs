using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WEB_SHOW_WRIST_STRAP.Hubs;

namespace WEB_SHOW_WRIST_STRAP.Services
{
    public class MonitoringBackgroundService : BackgroundService
    {
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitoringBackgroundService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

        public MonitoringBackgroundService(
            IHubContext<MonitoringHub> hubContext,
            IServiceScopeFactory scopeFactory,
            ILogger<MonitoringBackgroundService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (MonitoringHub.HasConnectedClients())
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var monitoringService = scope.ServiceProvider.GetRequiredService<IMonitoringService>();
                                var data = await monitoringService.GetAllPointsNowAsync();
                                if (data != null)
                                {
                                    // Serialize dữ liệu thành chuỗi JSON
                                    string jsonData = JsonSerializer.Serialize(data);
                                    await _hubContext.Clients.All.SendAsync("ReceiveMonitoringData", jsonData, stoppingToken);
                                    _logger.LogInformation("Sent monitoring data to clients at {Time}", DateTime.UtcNow);
                                }
                                else
                                {
                                    _logger.LogWarning("No data returned from GetAllPointsNowAsync at {Time}", DateTime.UtcNow);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogDebug("No clients connected, skipping data fetch at {Time}", DateTime.UtcNow);
                        }

                        await Task.Delay(_updateInterval, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("MonitoringBackgroundService was cancelled at {Time}", DateTime.UtcNow);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while fetching or sending monitoring data at {Time}", DateTime.UtcNow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "MonitoringBackgroundService stopped unexpectedly at {Time}", DateTime.UtcNow);
            }
        }
    }
}