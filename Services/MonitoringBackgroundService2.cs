using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WEB_SHOW_WRIST_STRAP.Hubs;

namespace WEB_SHOW_WRIST_STRAP.Services
{
    public class MonitoringBackgroundService2 : BackgroundService
    {
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitoringBackgroundService2> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

        public MonitoringBackgroundService2(
            IHubContext<MonitoringHub> hubContext,
            IServiceScopeFactory scopeFactory,
            ILogger<MonitoringBackgroundService2> logger)
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
                                var data = await monitoringService.GetAllPointsNowAsync2();
                                if (data != null)
                                {
                                    string jsonData = JsonSerializer.Serialize(data);
                                    // gửi riêng event khác để tránh trùng tên với hệ thống 1
                                    await _hubContext.Clients.All.SendAsync("ReceiveMonitoringData2", jsonData, stoppingToken);
                                    _logger.LogInformation("Sent monitoring data (System 2) at {Time}", DateTime.UtcNow);
                                }
                                else
                                {
                                    _logger.LogWarning("No data returned from GetAllPointsNowAsync2 at {Time}", DateTime.UtcNow);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogDebug("No clients connected (System 2) at {Time}", DateTime.UtcNow);
                        }

                        await Task.Delay(_updateInterval, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("MonitoringBackgroundService2 was cancelled at {Time}", DateTime.UtcNow);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching/sending data (System 2) at {Time}", DateTime.UtcNow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "MonitoringBackgroundService2 stopped unexpectedly at {Time}", DateTime.UtcNow);
            }
        }
    }
}
