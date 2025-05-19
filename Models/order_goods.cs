using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    [Table("order_goods")]
    public class OrderGoods
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Order")]
        [Column("order_id")]
        public long OrderId { get; set; }

        public Order Order { get; set; } = null!;

        [Required]
        [ForeignKey("Good")]
        [Column("good_id")]
        public long GoodId { get; set; }

        public Goods Good { get; set; } = null!;

        [Required]
        [Column("count_of_good")]
        [Display(Name = "Количество товара")]
        public int CountOfGood { get; set; }
    }
}
