using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.Text.Json.Serialization;

    [Table("goods")]
    public class Goods
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("name_of_good")]
        public string Name_Of_Good { get; set; }

        [ForeignKey("Category")]
        [Column("good_category")]
        public long Good_Category { get; set; }

        [ForeignKey("Type")]
        [Column("good_type")]
        public long? Good_Type { get; set; }

        [ForeignKey("Material")]
        [Column("material_type")]
        public long? Material_Type { get; set; }

        [Column("size")]
        public int? Size { get; set; }

        [Column("color")]
        public string? Color { get; set; }

        [Column("count_in_availability")]
        public long? Count_In_Availability { get; set; }

        [Column("price")]
        public long? Price { get; set; }

        [Column("is_set")]
        public bool IsSet { get; set; }

        [InverseProperty("Set")]
        public List<SetComposition> SetContents { get; set; } = new();

        [JsonIgnore]
        [BindNever]
        public virtual GoodCategory Category { get; set; }

        [JsonIgnore]
        [BindNever]
        public virtual GoodType Type { get; set; }

        [JsonIgnore]
        [BindNever]
        public virtual Material Material { get; set; }
        public ICollection<OrderGoods> OrderGoods { get; set; } = new List<OrderGoods>();
    }
}
