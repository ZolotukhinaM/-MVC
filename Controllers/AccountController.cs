using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Курсовая_работа_MVC.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Курсовая_работа_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string name, string password)
        {
            // Валидация пустых полей
            if (string.IsNullOrEmpty(name))
            {
                ViewBag.NameError = "Введите имя пользователя";
                return View();
            }
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.PasswordError = "Введите пароль";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Name == name);
            if (user == null)
            {
                ViewBag.Message = "Неверный логин или пароль";
                return View();
            }

            // Извлекаем соль и хеш из базы данных
            var parts = user.Password.Split('$');
            if (parts.Length != 2)
            {
                ViewBag.Message = "Ошибка хеширования пароля.";
                return View();
            }

            byte[] salt = Convert.FromBase64String(parts[0]); // Соль
            string storedHash = parts[1]; // Хеш
            // Хешируем введённый пароль с той же солью
            string hashedPassword = HashPasswordWithSalt(password, salt);

            if (hashedPassword != storedHash) // Сравнение хешей
            {
                ViewBag.Message = "Неверный логин или пароль";
                return View();
            }

            // Создание куки для аутентификации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "Seller")
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);
            // Перенаправляем на панели в зависимости от роли
            if (user.IsAdmin)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Метод для хеширования пароля с солью
        private string HashPasswordWithSalt(string password, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256, // Хеширование
                iterationCount: 100000, // Кол-во итераций
                numBytesRequested: 256 / 8)); // Длина выходного хеша

            return hashed;
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");  // Завершаем сессию
            return RedirectToAction("Login");  // Перенаправляем на страницу входа
        }
    }
}
