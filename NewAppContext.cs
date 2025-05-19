using System.Collections.Generic;
using Курсовая_работа_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Курсовая_работа_MVC
{
    public class NewAppContext : DbContext
    {
        public NewAppContext(DbContextOptions<NewAppContext> options) : base(options) { }

        public DbSet<Goods> Goods { get; set; }
        public DbSet<GoodCategory> GoodCategories { get; set; }
        public DbSet<GoodType> GoodTypes { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<SetComposition> SetComposition { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WayOfConnect> WaysOfConnect { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<ReceivingAnOrder> ReceivingAnOrder { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderGoods> OrderGoods { get; set; } // Не забудьте это!

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Goods>().ToTable("goods");

            modelBuilder.Entity<OrderGoods>()
                .HasIndex(og => new { og.OrderId, og.GoodId })
                .IsUnique();

            modelBuilder.Entity<OrderGoods>()
                .HasOne(og => og.Order)
                .WithMany(o => o.OrderGoods)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Удаляем товары вместе с заказом

            modelBuilder.Entity<OrderGoods>()
                .HasOne(og => og.Good)
                .WithMany(g => g.OrderGoods)
                .HasForeignKey(og => og.GoodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(o => o.ReceivingData)
                .HasColumnType("timestamp without time zone");

        }

    }
}
