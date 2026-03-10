using System.Collections.Generic;

namespace ShoeStore.ViewModels
{
    public class StaffDashboardViewModel
    {
        public bool IsAdmin { get; set; }
        public List<StaffSectionOption> Sections { get; set; } = new();
    }

    public class StaffSectionOption
    {
        public string RoleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
    }
}
