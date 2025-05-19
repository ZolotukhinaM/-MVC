using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")] 
        public long Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата получения")]
        [Column("receiving_data")]
        public DateTime ReceivingData { get; set; }


        [Required]
        [Column("customer_id")]
        [ForeignKey("Customer")]
        [Display(Name = "Клиент")]
        public long CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        [InverseProperty("Orders")]
        public Customer Customer { get; set; } = null!;


        [Required]
        [Column("payment_method")]
        [ForeignKey("PaymentMethod")]
        [Display(Name = "Метод оплаты")]
        public long PaymentMethod { get; set; }
        [ForeignKey("PaymentMethod")]
        [InverseProperty("Orders")]
        public PaymentMethod PaymentMethodNavigation { get; set; } = null!;



        [Column("method_of_receiving")]
        [ForeignKey("ReceivingAnOrder")]
        [Display(Name = "Способ получения")]
        public long? MethodOfReceiving { get; set; }
        public ReceivingAnOrder? ReceivingAnOrder { get; set; }

        [Column("address")] 
        [Display(Name = "Адрес")]
        public string? Address { get; set; }


        public ICollection<OrderGoods> OrderGoods { get; set; } = new List<OrderGoods>();
    }
    public class CreateOrderViewModel
    {
        public Order Order { get; set; }
        public List<OrderGoodsViewModel> Goods { get; set; } = new List<OrderGoodsViewModel>();
    }

    public class OrderGoodsViewModel
    {
        public long GoodId { get; set; }
        public int Quantity { get; set; }
    }

    public class EditOrderViewModel
    {
        public Order Order { get; set; }
        public List<EditOrderGoodsViewModel> OrderGoods { get; set; } = new List<EditOrderGoodsViewModel>();
        public SelectList AvailableGoods { get; set; }
    }

    public class EditOrderGoodsViewModel
    {
        public long Id { get; set; }
        public long GoodId { get; set; }
        public int CountOfGood { get; set; }
        public bool ToDelete { get; set; } = false;
    }
}
