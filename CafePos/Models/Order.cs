using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CafePos.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string OrderCode { get; set; } // Ví dụ: HD20260412001

        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        // Ai là người bán?
        public int UserId { get; set; }
        [ValidateNever]
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Precision(18, 2)]
        public decimal SubTotal { get; set; } // Tổng tiền hàng (chưa giảm)

        [Precision(18, 2)]
        public decimal DiscountAmount { get; set; } // Số tiền giảm giá (để decimal mới tính toán được)

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; } // Tiền khách phải trả = SubTotal - DiscountAmount

        public string OrderStatus { get; set; } // Ví dụ: Completed, Cancelled
        public string PaymentStatus { get; set; } // Ví dụ: Paid, Unpaid
        public string? PaymentMethod { get; set; } // Tiền mặt/Chuyển khoản

        public string? Note { get; set; }

        [ValidateNever]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}