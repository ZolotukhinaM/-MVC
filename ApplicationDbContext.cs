using System.Collections.Generic;
using Курсовая_работа_MVC.Models;
using Microsoft.EntityFrameworkCore;


namespace Курсовая_работа_MVC
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
