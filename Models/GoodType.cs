using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Курсовая_работа_MVC.Models
{
    [Table("good_types")]
    public class GoodType
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("good_type")]
        public string GoodTypeName { get; set; }
        public List<Goods>? Goods { get; set; } // Сделаем список товаров необязательным
    }
}