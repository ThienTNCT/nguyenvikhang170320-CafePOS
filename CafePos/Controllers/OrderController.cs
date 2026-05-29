using CafePos.Data;
using CafePos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace CafePos.Controllers
{
    public class OrderController : Controller
    {
        private readonly CafePosDbContext _context;

        public OrderController(CafePosDbContext context)
        {
            _context = context;
        }

        // Danh sách đơn
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(orders);
        }

        // Danh sách hóa đơn + khung xem ajax
        public async Task<IActionResult> Invoice()
        {
            var orders = await _context.Orders
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(orders);
        }

        // Chi tiết full page
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemToppings)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // Checkout - GET
       
        public async Task<IActionResult> Checkout(int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemToppings)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Checkout - POST
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int id,List<int>? selectedToppingIds,decimal discountAmount,string paymentMethod)
        {
            ViewBag.Toppings = await _context.Toppings
                .Where(x => x.IsActive)
                .ToListAsync();

            var order = await _context.Orders
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemToppings)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            // Chặn thanh toán lặp
            if (order.PaymentStatus == "Paid")
            {
                TempData["Message"] = "Đơn hàng này đã được thanh toán rồi";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(Detail), new { id = order.OrderId });
            }
            if (order.PaymentStatus == "Paid" || order.OrderStatus == "Completed")
            {
                TempData["Message"] = "Đơn hàng đã checkout trước đó";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(Detail), new { id = order.OrderId });
            }
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["Message"] = "Vui lòng chọn phương thức thanh toán";
                TempData["MessageType"] = "error";
                return View(order);
            }

            var firstItem = order.OrderItems.FirstOrDefault();
            if (firstItem == null)
            {
                TempData["Message"] = "Đơn hàng chưa có món nào";
                TempData["MessageType"] = "error";
                return View(order);
            }

            if (firstItem.OrderItemToppings == null)
                firstItem.OrderItemToppings = new List<OrderItemTopping>();

            // Thêm topping mới, tránh add trùng
            if (selectedToppingIds != null && selectedToppingIds.Any())
            {
                var toppings = await _context.Toppings
                    .Where(x => selectedToppingIds.Contains(x.ToppingId) && x.IsActive)
                    .ToListAsync();

                foreach (var topping in toppings)
                {
                    bool existed = firstItem.OrderItemToppings
                        .Any(x => x.ToppingId == topping.ToppingId);

                    if (!existed)
                    {
                        firstItem.OrderItemToppings.Add(new OrderItemTopping
                        {
                            ToppingId = topping.ToppingId,
                            ToppingNameSnapshot = topping.Name,
                            Price = topping.Price,
                            Quantity = 1,
                            TotalPrice = topping.Price
                        });
                    }
                }
            }

            // Tính lại tiền toàn bộ đơn
            decimal subTotal = 0;

            foreach (var item in order.OrderItems)
            {
                decimal toppingTotal = item.OrderItemToppings?.Sum(x => x.TotalPrice) ?? 0;
                item.LineTotal = (item.UnitPrice * item.Quantity) + toppingTotal;
                subTotal += item.LineTotal;
            }

            order.SubTotal = subTotal;
            order.DiscountAmount = discountAmount < 0 ? 0 : discountAmount;
            order.TotalAmount = order.SubTotal - order.DiscountAmount;

            if (order.TotalAmount < 0)
                order.TotalAmount = 0;

            order.PaymentMethod = paymentMethod;
            order.PaymentStatus = "Paid";
            order.OrderStatus = "Completed";

            await _context.SaveChangesAsync();

            TempData["Message"] = "Checkout thành công";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(Detail), new { id = order.OrderId });
        }

        // AJAX: trả partial hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetInvoicePartial(int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemToppings)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            return PartialView("_InvoicePartial", order);
        }

        // AJAX: cập nhật trạng thái thanh toán
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id, string paymentMethod)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

            order.PaymentStatus = "Paid";
            order.PaymentMethod = paymentMethod;
            order.OrderStatus = "Completed";

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thanh toán thành công"
            });
        }

        // In hóa đơn
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemToppings)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}

