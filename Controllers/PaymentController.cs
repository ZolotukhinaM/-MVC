using Microsoft.AspNetCore.Mvc;
using Курсовая_работа_MVC.Models;
using Курсовая_работа_MVC;
using Microsoft.EntityFrameworkCore;

public class PaymentController : Controller
{
    private readonly NewAppContext _context;

    public PaymentController(NewAppContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.PaymentMethod.ToListAsync();
        return View("~/Views/Orders/Payment/Index.cshtml", data);
    }

    public IActionResult Create()
    {
        return View("~/Views/Orders/Payment/Create.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentMethod method)
    {
        // Проверка на повторяющееся имя метода оплаты
        if (_context.PaymentMethod.Any(m => m.PaymentMethodName == method.PaymentMethodName))
        {
            ModelState.AddModelError(nameof(method.PaymentMethodName), "Метод оплаты с таким именем уже существует.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(method);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Orders/Payment/Create.cshtml", method);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await _context.PaymentMethod.FindAsync(id);
        if (item == null) return NotFound();
        return View("~/Views/Orders/Payment/Edit.cshtml", item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PaymentMethod method)
    {
        if (id != method.Id) return NotFound();
        // Проверка на повторяющееся имя метода оплаты
        if (_context.PaymentMethod.Any(m => m.PaymentMethodName == method.PaymentMethodName))
        {
            ModelState.AddModelError(nameof(method.PaymentMethodName), "Метод оплаты с таким именем уже существует.");
        }
        if (ModelState.IsValid)
        {
            _context.Update(method);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View("~/Views/Orders/Payment/Edit.cshtml", method);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _context.PaymentMethod.FindAsync(id);
        if (item != null)
        {
            _context.PaymentMethod.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}


