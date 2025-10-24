using Microsoft.EntityFrameworkCore;
using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly DataPointContext _context; // Thay bằng tên DbContext của bạn

        public MonitoringService(DataPointContext context)
        {
            _context = context;
        }

        public async Task<List<DataNow>> GetAllPointsNowAsync()
        {
            try
            {
                // Lấy chỉ các ListPoint có Type = 2
                var type2Points = await _context.ListPoints
                    .AsNoTracking()
                    .Where(p => p.Type == "2")
                    .Select(p => new { p.IdPoint, p.IdLine })
                    .ToListAsync();

                // Lấy toàn bộ DataNow
                var pointsNow = await _context.DataNows
                    .AsNoTracking()
                    .OrderByDescending(p => p.IdPoint)
                    .ToListAsync();

                // Gán Alarm = 7 nếu IdPoint nằm trong danh sách Type = 2
                var type2PointSet = new HashSet<(int IdPoint, int IdLine)>(
                    type2Points.Select(p => (p.IdPoint, p.IdLine))
                );
                foreach (var point in pointsNow)
                {
                    if (type2PointSet.Contains((point.IdPoint, point.IdLine)))
                    {
                        point.Alarm = 7;
                    }
                }

                return pointsNow;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error in service");
                throw; // Hoặc handle tùy ý
            }
        }
        public async Task<List<DataNow2>> GetAllPointsNowAsync2()
        {
            try
            {
                // Lấy chỉ các ListPoint có Type = 2
                var type2Points = await _context.ListPoints2
                    .AsNoTracking()
                    .Where(p => p.Type == "2")
                    .Select(p => new { p.IdPoint, p.IdLine })
                    .ToListAsync();

                // Lấy toàn bộ DataNow
                var pointsNow = await _context.DataNows2
                    .AsNoTracking()
                    .OrderByDescending(p => p.IdPoint)
                    .ToListAsync();

                // Gán Alarm = 7 nếu IdPoint nằm trong danh sách Type = 2
                var type2PointSet = new HashSet<(int IdPoint, int IdLine)>(
                    type2Points.Select(p => (p.IdPoint, p.IdLine))
                );
                foreach (var point in pointsNow)
                {
                    if (type2PointSet.Contains((point.IdPoint, point.IdLine)))
                    {
                        point.Alarm = 7;
                    }
                }

                return pointsNow;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error in service");
                throw; // Hoặc handle tùy ý
            }
        }
    }
}