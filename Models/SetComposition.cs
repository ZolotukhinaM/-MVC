using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Курсовая_работа_MVC.Models;
[Table("sets_composition")]
public class SetComposition
{
    [Column("id")]
    public long Id { get; set; }

    [Column("set_id")]
    public long SetId { get; set; }

    [Column("good_id")]
    public long GoodId { get; set; }

    [Column("count_of_good")]
    public int CountOfGood { get; set; }

    [ForeignKey("SetId")]
    public Goods Set { get; set; } 

    [ForeignKey("GoodId")]
    public Goods Good { get; set; } 
}