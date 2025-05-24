using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Курсовая_работа_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Курсовая_работа_MVC.Controllers
{
    // Контроллер для управления пользователями системы
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }
        private string HashPasswordWithSalt(string password, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        [HttpPost]
        public IActionResult CreateUser(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Генерируем соль
                byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

                // Хешируем пароль с солью
                string hashedPassword = HashPasswordWithSalt(model.Password, salt);

                // Сохраняем соль и хеш в базе данных
                var user = new User
                {
                    Name = model.Name,
                    Password = Convert.ToBase64String(salt) + "$" + hashedPassword,
                    IsAdmin = model.IsAdmin,
                    RegistrationDate = DateTime.UtcNow
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                ViewBag.Success = "Сотрудник успешно добавлен!";
                return View();
            }
            catch (DbUpdateException ex)
            {
                // Проверка на дублирование имени
                if (ex.InnerException is PostgresException postgresEx
                    && postgresEx.SqlState == "23505") // Код ошибки для уникального ограничения
                {
                    ModelState.AddModelError("Name", "Работник с таким именем уже существует.");
                }
                else
                {
                    ModelState.AddModelError("", "Произошла ошибка при добавлении сотрудника.");
                }
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult ToggleAdmin(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsAdmin = !user.IsAdmin;
                _context.SaveChanges();
            }

            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = $"Сотрудник «{user.Name}» успешно удалён.";
            }

            return RedirectToAction("UserManagement");
        }
        public IActionResult UserManagement()
        {
            // Получаем всех пользователей и сортируем по имени в алфавитном порядке
            var users = _context.Users
                .OrderBy(u => u.Name)
                .ToList();

            return View(users);
        }
    }
}
