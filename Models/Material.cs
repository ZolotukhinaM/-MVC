using System.ComponentModel.DataAnnotations.Schema;
using Курсовая_работа_MVC.Models;

namespace Курсовая_работа_MVC.Models
{
    [Table("materials")]
    public class Material
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("type_of_material")]
        public string TypeOfMaterial { get; set; }
        public List<Goods>? Goods { get; set; } // Сделаем список товаров необязательным
    }
}