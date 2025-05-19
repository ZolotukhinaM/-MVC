// Models/ReceivingAnOrder.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    [Table("receiving_an_order")]
    public class ReceivingAnOrder
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("type_of_receiving")]
        public string TypeOfReceiving { get; set; }
    }
}
