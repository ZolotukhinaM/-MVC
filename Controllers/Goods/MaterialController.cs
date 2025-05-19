using Курсовая_работа_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Курсовая_работа_MVC.Controllers.Goods
{
    [Authorize(Roles = "Admin,Seller")]

    public class MaterialController : Controller
    {
        private readonly NewAppContext _context;

        public MaterialController(NewAppContext context)
        {
            _context = context;
        }

        // Метод для отображения списка материалов
        public IActionResult Index()
        {
            var materials = _context.Materials.ToList();
            return View("/Views/Good/Material/Index.cshtml", materials); 
        }

        // Метод для отображения формы создания материала
        [HttpGet]
        public IActionResult Create()
        {
            return View("/Views/Good/Material/Create.cshtml", new Material());
        }

        // Метод для сохранения нового материала
        [HttpPost]
        public IActionResult Create(Material material)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже такой материал
                if (_context.Materials.Any(m => m.TypeOfMaterial == material.TypeOfMaterial))
                {
                    ModelState.AddModelError("TypeOfMaterial", "Материал с таким названием уже существует.");
                }
                else
                {
                    // Логируем приходящие данные для отладки
                    Console.WriteLine($"Создание материала: {material.TypeOfMaterial}");
                    _context.Materials.Add(material);
                    _context.SaveChanges();
                    return RedirectToAction("Index"); // Перенаправление на список материалов
                }
            }

            // Логируем ошибки, если модель не валидна
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Ошибка: {error.ErrorMessage}");
            }

            // Возвращаем форму с уже введёнными данными, чтобы пользователь мог исправить ошибки
            return View("/Views/Good/Material/Create.cshtml", material);
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            var material = _context.Materials.Find(id);
            if (material == null) return NotFound();
            return View("/Views/Good/Material/Edit.cshtml", material);
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Edit(Material material)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже такой материал с другим идентификатором
                if (_context.Materials.Any(m => m.TypeOfMaterial == material.TypeOfMaterial && m.Id != material.Id))
                {
                    ModelState.AddModelError("TypeOfMaterial", "Материал с таким названием уже существует.");
                }
                else
                {
                    _context.Materials.Update(material);
                    _context.SaveChanges();
                    return RedirectToAction("Index"); // Перенаправление на список материалов
                }
            }

            // Возвращаем форму с уже введёнными данными, чтобы пользователь мог исправить ошибки
            return View("/Views/Good/Material/Edit.cshtml", material);
        }

        // Метод для удаления материала
        [HttpPost]
        public IActionResult Delete(long id)
        {
            var material = _context.Materials.Find(id);
            if (material == null) return NotFound();

            _context.Materials.Remove(material);
            _context.SaveChanges();
            return RedirectToAction("Index");  // Перенаправление на список материалов
        }
    }
}
