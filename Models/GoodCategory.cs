using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    [Table("good_categories")] // имя таблицы в базе
    public class GoodCategory
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("good_category")] // имя поля в таблице
        public string good_category { get; set; }

        public List<Goods>? Goods { get; set; } // Сделаем список товаров необязательным
    }
}
