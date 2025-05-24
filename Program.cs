using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Курсовая_работа_MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddControllersWithViews();
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDbContext<NewAppContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("GoodsConnection")));

            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", config =>
                {
                    config.LoginPath = "/Account/Login"; // Для неавторизированных
                    config.LogoutPath = "/Account/Logout"; // Выход
                    config.AccessDeniedPath = "/Account/AccessDenied"; // Если нет доступа
                });

            builder.Services.AddAuthorization();


            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
