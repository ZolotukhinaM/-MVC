using Курсовая_работа_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Курсовая_работа_MVC.Controllers.Goods
{
    [Authorize(Roles = "Admin,Seller")]

    public class GoodTypeController : Controller
    {
        private readonly NewAppContext _context;

        public GoodTypeController(NewAppContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var goodTypes = _context.GoodTypes.ToList();
            return View("/Views/Good/GoodType/Index.cshtml", goodTypes);  // Указание полного пути
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("/Views/Good/GoodType/Create.cshtml", new GoodType());  // Указание полного пути
        }

        // Метод для сохранения нового типа товара
        [HttpPost]
        public IActionResult Create(GoodType type)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже такой тип товара
                if (_context.GoodTypes.Any(g => g.GoodTypeName == type.GoodTypeName))
                {
                    ModelState.AddModelError("GoodTypeName", "Тип товара с таким названием уже существует.");
                }
                else
                {
                    Console.WriteLine($"Создание типа товара: {type.GoodTypeName}");
                    _context.GoodTypes.Add(type);
                    _context.SaveChanges();
                    return RedirectToAction("Index"); // Перенаправление на список типов товаров
                }
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Ошибка: {error.ErrorMessage}");
            }
            return View("/Views/Good/GoodType/Create.cshtml", type); // Указание полного пути
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            var type = _context.GoodTypes.Find(id);
            if (type == null) return NotFound();
            return View("/Views/Good/GoodType/Edit.cshtml", type);  // // Возвращаем форму с ошибками, если модель не валидна
        }

        // Метод для сохранения изменений в типе товара
        [HttpPost]
        public IActionResult Edit(GoodType type)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже такой тип товара с другим идентификатором
                if (_context.GoodTypes.Any(g => g.GoodTypeName == type.GoodTypeName && g.Id != type.Id))
                {
                    ModelState.AddModelError("GoodTypeName", "Тип товара с таким названием уже существует.");
                }
                else
                {
                    _context.GoodTypes.Update(type);
                    _context.SaveChanges();
                    return RedirectToAction("Index"); // Перенаправление на список типов товаров
                }
            }
            return View("/Views/Good/GoodType/Edit.cshtml", type); // Указание полного пути
        }
        [HttpPost]
        public IActionResult Delete(long id)
        {
            var type = _context.GoodTypes.Find(id);
            if (type == null) return NotFound();

            _context.GoodTypes.Remove(type);
            _context.SaveChanges();
            return RedirectToAction("Index");  // Перенаправление на список типов товаров
        }
    }
}
