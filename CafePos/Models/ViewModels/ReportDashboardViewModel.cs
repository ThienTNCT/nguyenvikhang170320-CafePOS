using CafePos.Models.ViewModels;

namespace CafePos.ViewModels
{
    public class ReportDashboardViewModel
    {
        public int SelectedYear { get; set; }
        public decimal TotalRevenueThisYear { get; set; }
        public int TotalOrdersThisYear { get; set; }

        public List<RevenueByMonthViewModel> MonthlyRevenues { get; set; } = new();
        public List<RevenueByYearViewModel> YearlyRevenues { get; set; } = new();
        public List<BestSellingProductViewModel> BestSellingProducts { get; set; } = new();
    }
}