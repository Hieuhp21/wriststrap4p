using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Core.ViewModels
{
    public class ListPlcViewModel
    {
        public ListPlc ListPlc { get; set; }
        public List<ActUnitType> ActUnitTypes { get; set; }
        public List<ActProtocolType> ActProtocolTypes { get; set; }
    }
}
