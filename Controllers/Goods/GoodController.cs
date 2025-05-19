using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Курсовая_работа_MVC.Models;
using Курсовая_работа_MVC;
using System.Text.Json;

public class GoodController : Controller
{
    private readonly NewAppContext _context;

    public GoodController(NewAppContext context)
    {
        _context = context;
    }
    // Страница со списком товаров, поиск и CRUD операции
    public IActionResult Index(string name, string color, long? categoryId, long? typeId, long? materialId)
    {
        var query = _context.Goods
            .Include(g => g.Category)
            .Include(g => g.Type)
            .Include(g => g.Material)
            .AsQueryable();

        // Применение фильтров по запросу
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(g => g.Name_Of_Good.Contains(name));

        if (!string.IsNullOrWhiteSpace(color))
            query = query.Where(g => g.Color != null && g.Color.Contains(color));

        // Обработка категории: если categoryId пустой, фильтрация по категории не применяется
        if (categoryId.HasValue && categoryId.Value > 0) // Проверяем, что значение categoryId указано
            query = query.Where(g => g.Good_Category == categoryId);

        if (typeId.HasValue && typeId.Value > 0)
            query = query.Where(g => g.Good_Type == typeId);

        if (materialId.HasValue && materialId.Value > 0)
            query = query.Where(g => g.Material_Type == materialId);

        // Получаем данные для фильтров (категории, типы, материалы)
        ViewBag.Categories = _context.GoodCategories.ToList();
        ViewBag.Types = _context.GoodTypes.ToList();
        ViewBag.Materials = _context.Materials.ToList();

        // Возвращаем представление со списком товаров и фильтрами
        return View(query.ToList());
    }
    public IActionResult AddItemsToSet(long setId)
    {
        var set = _context.Goods.Find(setId);
        if (set == null || !set.IsSet)
        {
            return NotFound();
        }

        // Получаем список всех товаров (кроме наборов)
        var goods = _context.Goods
            .Where(g => !g.IsSet)
            .Include(g => g.Category)
            .ToList();

        ViewBag.Set = set;
        return View(goods); // Передаем список товаров как модель
    }

    [HttpPost]
    public IActionResult AddItemToSet(long setId, long goodId, int count)
    {
        try
        {
            // Проверяем что набор существует и это действительно набор
            var set = _context.Goods.FirstOrDefault(g => g.Id == setId && g.IsSet);
            if (set == null)
                return Json(new { success = false, error = "Набор не найден или не является набором" });

            // Проверяем что товар существует и это не набор
            var good = _context.Goods.FirstOrDefault(g => g.Id == goodId && !g.IsSet);
            if (good == null)
                return Json(new { success = false, error = "Товар не найден или является набором" });

            // Проверяем нет ли уже этого товара в наборе
            if (_context.SetComposition.Any(sc => sc.SetId == setId && sc.GoodId == goodId))
                return Json(new { success = false, error = "Этот товар уже есть в наборе" });

            var setItem = new SetComposition
            {
                SetId = setId,
                GoodId = goodId,
                CountOfGood = count
            };

            _context.SetComposition.Add(setItem);
            _context.SaveChanges();

            // Получаем обновленный список товаров в наборе
            var updatedItems = _context.SetComposition
                .Where(sc => sc.SetId == setId)
                .Include(sc => sc.Good)
                .Select(sc => new {
                    id = sc.Id,
                    goodName = sc.Good.Name_Of_Good,
                    count = sc.CountOfGood,
                    goodId = sc.GoodId
                })
                .ToList();

            return Json(new
            {
                success = true,
                items = updatedItems // Возвращаем весь обновленный список
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    [HttpGet]
    public IActionResult GetSetItems(long setId)
    {
        try
        {
            Console.WriteLine($"Запрос товаров для набора {setId}");
            var items = _context.SetComposition
                .Where(sc => sc.SetId == setId)
                .Include(sc => sc.Good)
                .ToList();

            Console.WriteLine($"Найдено товаров: {items.Count}");

            var result = items.Select(sc => new {
                id = sc.Id,
                goodName = sc.Good.Name_Of_Good,
                count = sc.CountOfGood,
                goodId = sc.GoodId
            }).ToList();

            return Json(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex}");
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [HttpDelete("Good/RemoveSetItem/{id}")]
    public IActionResult RemoveSetItem(long id)
    {
        Console.WriteLine($"Получен запрос на удаление элемента набора: {id}");

        try
        {
            var item = _context.SetComposition.Find(id);
            if (item == null)
            {
                Console.WriteLine("Элемент не найден");
                return Json(new { success = false, error = "Элемент не найден" });
            }

            _context.SetComposition.Remove(item);
            _context.SaveChanges();
            Console.WriteLine("Элемент успешно удален");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            return Json(new
            {
                success = false,
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    [HttpGet]
    public IActionResult Create()
    {
        // Загружаем необходимые данные для выпадающих списков
        ViewBag.Categories = _context.GoodCategories.ToList();
        ViewBag.Types = _context.GoodTypes.ToList();
        ViewBag.Materials = _context.Materials.ToList();
        ViewBag.AllGoods = _context.Goods.ToList();

        return PartialView("_CreateGood", new Goods());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Goods goods)
    {
        ModelState.Remove(nameof(Goods.Category));
        ModelState.Remove(nameof(Goods.Type));
        ModelState.Remove(nameof(Goods.Material));

        if (ModelState.IsValid)
        {
            // Проверка на дубликат
            if (_context.Goods.Any(g => g.Name_Of_Good == goods.Name_Of_Good))
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string>
                {
                    { "Name_Of_Good", "Товар с таким названием уже существует." }
                }
                });
            }
            try
            {
                _context.Goods.Add(goods);
                _context.SaveChanges();

                if (goods.IsSet)
                {
                    return Json(new
                    {
                        success = true,
                        isSet = true,
                        setId = goods.Id
                    });
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "Ошибка при сохранении: " + ex.Message
                });
            }
        }

        // Собираем все ошибки валидации
        var errors = ModelState.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.FirstOrDefault()?.ErrorMessage
        );

        return Json(new
        {
            success = false,
            errors = errors
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Goods goods)
    {
        // Удаляем ненужные проверки для навигационных свойств
        ModelState.Remove(nameof(Goods.Category));
        ModelState.Remove(nameof(Goods.Type));
        ModelState.Remove(nameof(Goods.Material));

        if (ModelState.IsValid)
        {
            // Проверка на дубликат названия (исключая текущий товар)
            if (_context.Goods.Any(g => g.Name_Of_Good == goods.Name_Of_Good && g.Id != goods.Id))
            {
                ModelState.AddModelError("Name_Of_Good", "Товар с таким названием уже существует.");
            }
            else
            {
                try
                {
                    _context.Update(goods);
                    _context.SaveChanges();
                    return RedirectToAction("Index"); // Успешное сохранение - переходим к списку
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
                }
            }
        }

        // Заполняем ViewBag для выпадающих списков при ошибке
        ViewBag.Categories = _context.GoodCategories.ToList();
        ViewBag.Types = _context.GoodTypes.ToList();
        ViewBag.Materials = _context.Materials.ToList();
        return View(goods); // Возвращаем представление с ошибками
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditGood(Goods goods)
    {
        ModelState.Remove(nameof(Goods.Category));
        ModelState.Remove(nameof(Goods.Type));
        ModelState.Remove(nameof(Goods.Material));

        if (ModelState.IsValid)
        {
            // Проверка на дубликат (исключая текущий товар)
            if (_context.Goods.Any(g => g.Name_Of_Good == goods.Name_Of_Good && g.Id != goods.Id))
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string>
                {
                    { "Name_Of_Good", "Товар с таким названием уже существует." }
                }
                });
            }
            try
            {
                _context.Update(goods);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "Ошибка при сохранении: " + ex.Message
                });
            }
        }

        // Собираем все ошибки валидации
        var errors = ModelState.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.FirstOrDefault()?.ErrorMessage
        );
        // Заполняем ViewBag для выпадающих списков
        ViewBag.Categories = _context.GoodCategories.ToList();
        ViewBag.Types = _context.GoodTypes.ToList();
        ViewBag.Materials = _context.Materials.ToList();

        return Json(new
        {
            success = false,
            errors = errors
        });
    }
    // Удаление товара
    public IActionResult Delete(long id)
    {
        var goods = _context.Goods.Find(id);
        if (goods == null)
        {
            return NotFound();
        }
        return PartialView("_DeleteGood", goods);
    }

    // Подтверждение удаления товара
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(long id)
    {
        var goods = _context.Goods.Find(id);
        _context.Goods.Remove(goods);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Details(long id)
    {
        var set = _context.Goods
            .Include(g => g.SetContents)
                .ThenInclude(og => og.Good)
            .FirstOrDefault(g => g.Id == id && g.IsSet);

        if (set == null)
            return NotFound();

        return View(set); //создадим соответствующее представление
    }
}
