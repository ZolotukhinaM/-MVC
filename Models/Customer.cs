using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Курсовая_работа_MVC.Models
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("surname")]
        public string? Surname { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("phone_number")]
        public long PhoneNumber { get; set; }

        [Column("method_of_communication")]
        public long? MethodOfCommunicationId { get; set; }

        [ForeignKey("MethodOfCommunicationId")]
        public WayOfConnect? MethodOfCommunication { get; set; }
        [InverseProperty("Customer")]
        public List<Order> Orders { get; set; } = new();

    }

    [Table("ways_of_connect")]
    public class WayOfConnect
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Column("way")]
        public string Way { get; set; }

        public List<Customer> Customers { get; set; } = new();
    }
}
