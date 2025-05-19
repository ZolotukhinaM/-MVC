// Models/PaymentMethod.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    [Table("payment_methods")]
    public class PaymentMethod
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("payment_method")]
        public string PaymentMethodName { get; set; }

        [InverseProperty("PaymentMethodNavigation")]
        public List<Order> Orders { get; set; } = new();
    }
}
