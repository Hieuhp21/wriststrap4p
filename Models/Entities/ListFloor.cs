using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class ListFloor
    {
        public int IdFloor { get; set; }
        public string NameFloor { get; set; } = null!;
        public string? Note { get; set; }
    }
}
