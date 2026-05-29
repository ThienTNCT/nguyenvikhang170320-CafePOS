using CafePos.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CafePos.Models
{
    public class OrderItemTopping
    {
        [Key]
        public int Id { get; set; }

        public int OrderItemId { get; set; }
        [ValidateNever]
        [ForeignKey("OrderItemId")] // Chỉ định rõ ràng khóa ngoại
        public virtual OrderItem OrderItem { get; set; }

        public int ToppingId { get; set; }

        [Required] // Bắt buộc phải lưu tên để sau này in hóa đơn
        public string ToppingNameSnapshot { get; set; }

        [ValidateNever]
        [ForeignKey("ToppingId")] // Chỉ định rõ ràng khóa ngoại
        public virtual Topping Topping { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; } // Giá 1 phần topping lúc bán

        public int Quantity { get; set; } // Số lượng phần topping (thường là 1)

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; } // = Price * Quantity
    }
}