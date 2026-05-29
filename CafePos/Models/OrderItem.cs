using CafePos.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CafePos.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        [ValidateNever]
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }
        [ValidateNever]
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public int? ProductSizeId { get; set; }

        public int Quantity { get; set; }

        [Required]
        public string ProductNameSnapshot { get; set; } // Luôn phải có tên món

        public string? SizeNameSnapshot { get; set; } // Size có thể null

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; } // Giá gốc (món + size)

        [Precision(18, 2)]
        public decimal LineTotal { get; set; } // (Giá gốc + Tổng topping) * Quantity

        public string? ItemNote { get; set; } // Ghi chú có thể để trống

        // Link tới danh sách topping của món này
        [ValidateNever]
        public virtual ICollection<OrderItemTopping> OrderItemToppings { get; set; }
    }
}