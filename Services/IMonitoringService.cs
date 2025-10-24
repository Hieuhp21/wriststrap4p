using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Services
{
    public interface IMonitoringService
    {
        Task<List<DataNow>> GetAllPointsNowAsync();
        Task<List<DataNow2>> GetAllPointsNowAsync2();
    }
}
