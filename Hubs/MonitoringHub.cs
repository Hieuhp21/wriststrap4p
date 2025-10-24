using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WEB_SHOW_WRIST_STRAP.Hubs
{
    public class MonitoringHub : Hub
    {
        // Danh sách để theo dõi client kết nối (tùy chọn, dùng để tối ưu)
        private static readonly HashSet<string> _connectedClients = new HashSet<string>();

        // Gọi khi client kết nối
        public override async Task OnConnectedAsync()
        {
            _connectedClients.Add(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        // Gọi khi client ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connectedClients.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Phương thức để kiểm tra xem có client nào kết nối không
        public static bool HasConnectedClients() => _connectedClients.Any();
    }
}