using CafePos.Data;
using CafePos.Models;
using CafePos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Controllers
{
    [Authorize(Roles = "Employee")]
    public class RevenueController : Controller
    {
        private readonly CafePosDbContext _context;

        public RevenueController(CafePosDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;

            var paidOrders = _context.Orders
                .Where(x => x.CreatedDate.Year == selectedYear
                         && (x.PaymentStatus == "Paid" || x.PaymentStatus == "Đã thanh toán"
                             || x.OrderStatus == "Completed" || x.OrderStatus == "Hoàn thành"));

            var monthlyRevenue = await paidOrders
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .Select(g => new RevenueByMonthViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalOrders = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var yearlyRevenue = await _context.Orders
                .Where(x => x.PaymentStatus == "Paid" || x.PaymentStatus == "Đã thanh toán"
                         || x.OrderStatus == "Completed" || x.OrderStatus == "Hoàn thành")
                .GroupBy(x => x.CreatedDate.Year)
                .Select(g => new RevenueByYearViewModel
                {
                    Year = g.Key,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalOrders = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            var bestSellingProducts = await _context.OrderItems
                .Include(x => x.Order)
                .Where(x => x.Order != null &&
                           (x.Order.PaymentStatus == "Paid" || x.Order.PaymentStatus == "Đã thanh toán"
                         || x.Order.OrderStatus == "Completed" || x.Order.OrderStatus == "Hoàn thành"))
                .GroupBy(x => new { x.ProductId, x.ProductNameSnapshot })
                .Select(g => new BestSellingProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductNameSnapshot,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(10)
                .ToListAsync();

            var dashboard = new ReportDashboardViewModel
            {
                SelectedYear = selectedYear,
                TotalRevenueThisYear = monthlyRevenue.Sum(x => x.TotalRevenue),
                TotalOrdersThisYear = monthlyRevenue.Sum(x => x.TotalOrders),
                MonthlyRevenues = monthlyRevenue,
                YearlyRevenues = yearlyRevenue,
                BestSellingProducts = bestSellingProducts
            };

            return View(dashboard);
        }
    }
}