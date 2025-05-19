using Microsoft.AspNetCore.Mvc;
using Курсовая_работа_MVC.Models;
using Курсовая_работа_MVC;
using Microsoft.EntityFrameworkCore;

public class ReceivingController : Controller
{
    private readonly NewAppContext _context;

    public ReceivingController(NewAppContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.ReceivingAnOrder.ToListAsync();
        return View("~/Views/Orders/Receiving/Index.cshtml", data);
    }

    public IActionResult Create()
    {
        return View("~/Views/Orders/Receiving/Create.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReceivingAnOrder receiving)
    {
        // Проверка на повторяющееся имя метода получения
        if (_context.ReceivingAnOrder.Any(r => r.TypeOfReceiving == receiving.TypeOfReceiving))
        {
            ModelState.AddModelError(nameof(receiving.TypeOfReceiving), "Метод с таким именем уже существует.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(receiving);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Orders/Receiving/Create.cshtml", receiving);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _context.ReceivingAnOrder.FindAsync(id);
        if (item == null) return NotFound();
        return View("~/Views/Orders/Receiving/Edit.cshtml", item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ReceivingAnOrder receiving)
    {
        if (id != receiving.Id) return NotFound();

        // Проверка на повторяющееся имя метода получения, исключая текущий объект по Id
        if (_context.ReceivingAnOrder.Any(r => r.TypeOfReceiving == receiving.TypeOfReceiving && r.Id != id))
        {
            ModelState.AddModelError(nameof(receiving.TypeOfReceiving), "Метод с таким именем уже существует.");
        }

        if (ModelState.IsValid)
        {
            _context.Update(receiving);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Orders/Receiving/Edit.cshtml", receiving);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _context.ReceivingAnOrder.FindAsync(id);
        if (item != null)
        {
            _context.ReceivingAnOrder.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
