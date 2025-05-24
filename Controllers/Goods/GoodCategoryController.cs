using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Курсовая_работа_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Курсовая_работа_MVC.Controllers.Goods
{
    [Authorize(Roles = "Admin,Seller")]

    public class GoodCategoryController : Controller
    {
        private readonly NewAppContext _context;

        public GoodCategoryController(NewAppContext context)
        {
            _context = context;
        }

        // Метод для отображения списка категорий
        public IActionResult Index()
        {
            var categories = _context.GoodCategories.ToList();
            return View("/Views/Good/GoodCategory/Index.cshtml", categories);
        }

        // Метод для отображения формы создания категории
        [HttpGet]
        public IActionResult Create()
        {
            return View("/Views/Good/GoodCategory/Create.cshtml", new GoodCategory());
        }
        [HttpPost]
        public IActionResult Create(GoodCategory category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Логируем приходящие данные для отладки
                    Console.WriteLine($"Создание категории: {category.good_category}");

                    _context.GoodCategories.Add(category);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Категория успешно создана.";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    // Проверка на дублирование категории
                    if (ex.InnerException is PostgresException postgresEx
                        && postgresEx.SqlState == "23505") // Код ошибки для уникального ограничения
                    {
                        ModelState.AddModelError("good_category", "Категория с таким именем уже существует.");
                        TempData["ErrorMessage"] = "Ошибка: категория с таким именем уже существует.";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Произошла ошибка при создании категории.");
                        TempData["ErrorMessage"] = "Ошибка при создании категории. Пожалуйста, попробуйте еще раз.";
                    }
                }
            }

            // Логируем ошибки, если модель не валидна
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Ошибка: {error.ErrorMessage}");
            }
            return View("/Views/Good/GoodCategory/Create.cshtml", category);
        }

        // Метод для отображения формы редактирования категории
        [HttpGet]
        public IActionResult Edit(long id)
        {
            var category = _context.GoodCategories.Find(id);
            if (category == null) return NotFound();
            return View("/Views/Good/GoodCategory/Edit.cshtml", category); 
        }

        [HttpPost]
        public IActionResult Edit(GoodCategory category)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже такая категория с другим идентификатором
                if (_context.GoodCategories.Any(c => c.good_category == category.good_category && c.Id != category.Id))
                {
                    ModelState.AddModelError("good_category", "Категория с таким названием уже существует."); 
                }
                else
                {
                    _context.GoodCategories.Update(category);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View("/Views/Good/GoodCategory/Edit.cshtml", category);
        }

        // Метод для удаления категории
        [HttpPost]
        public IActionResult Delete(long id)
        {
            var category = _context.GoodCategories.Find(id);
            if (category == null) return NotFound();

            _context.GoodCategories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
