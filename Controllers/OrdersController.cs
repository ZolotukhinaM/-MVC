using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Курсовая_работа_MVC.Models;

namespace Курсовая_работа_MVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly NewAppContext _context;

        public OrdersController(NewAppContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
            bool showPastOrders = false,
            string sortOrder = "asc",
            long? receivingMethodId = null,
             string customerName = null)
        {
            var today = DateTime.Today;

            // Создаем базовый запрос с Include
            var query = _context.Order
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethodNavigation)
                .Include(o => o.ReceivingAnOrder)
                .AsQueryable(); // Явно указываем тип IQueryable<Order>

            // Фильтрация по имени клиента
            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(o =>
                    o.Customer.Name.Contains(customerName) ||
                    o.Customer.Surname.Contains(customerName));
            }

            // Фильтрация по дате
            if (!showPastOrders)
            {
                query = query.Where(o => o.ReceivingData.Date >= today);
            }

            // Фильтрация по способу получения
            if (receivingMethodId.HasValue)
            {
                query = query.Where(o => o.MethodOfReceiving == receivingMethodId);
            }

            // Сортировка
            query = sortOrder == "desc"
                ? query.OrderByDescending(o => o.ReceivingData)
                : query.OrderBy(o => o.ReceivingData);

            var orders = await query.ToListAsync();

            // Получаем список способов получения для dropdown
            ViewData["ReceivingMethods"] = new SelectList(
                _context.ReceivingAnOrder,
                "Id",
                "TypeOfReceiving",
                receivingMethodId);

            ViewData["ShowPastOrders"] = showPastOrders;
            ViewData["SortOrder"] = sortOrder;
            ViewData["SelectedReceivingMethod"] = receivingMethodId;
            ViewData["CustomerName"] = customerName; // Передаем введенное имя в представление

            return View(orders);
        }

        [HttpGet]
        public IActionResult Create(bool customerAdded = false, long? newCustomerId = null)
        {
            // Получаем только товары с положительным количеством
            var availableGoods = _context.Goods
                .Where(g => g.Count_In_Availability > 0)
                .ToList();

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethod, "Id", "PaymentMethodName");
            ViewData["MethodOfReceiving"] = new SelectList(_context.ReceivingAnOrder, "Id", "TypeOfReceiving");
            ViewData["Goods"] = new SelectList(availableGoods, "Id", "Name_Of_Good");

            if (customerAdded && newCustomerId.HasValue)
            {
                ViewData["SelectedCustomerId"] = newCustomerId.Value;
            }

            return View(new CreateOrderViewModel
            {
                Goods = new List<OrderGoodsViewModel> { new OrderGoodsViewModel() }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            try
            {
                // Предварительная проверка наличия всех товаров
                foreach (var goodsItem in model.Goods)
                {
                    if (goodsItem.GoodId > 0 && goodsItem.Quantity > 0)
                    {
                        var good = await _context.Goods
                            .Include(g => g.SetContents)
                            .FirstOrDefaultAsync(g => g.Id == goodsItem.GoodId);

                        if (good == null)
                        {
                            ModelState.AddModelError("", $"Товар с ID {goodsItem.GoodId} не найден");
                            continue;
                        }

                        // Проверка для обычного товара
                        if (!good.IsSet && (good.Count_In_Availability == null || good.Count_In_Availability < goodsItem.Quantity))
                        {
                            ModelState.AddModelError("",
                                $"Недостаточно товара '{good.Name_Of_Good}' в наличии (требуется: {goodsItem.Quantity}, доступно: {good.Count_In_Availability ?? 0})");
                            continue;
                        }

                        // Проверка для набора товаров
                        if (good.IsSet)
                        {
                            foreach (var component in good.SetContents)
                            {
                                var includedGood = await _context.Goods.FindAsync(component.GoodId);
                                if (includedGood == null) continue;

                                int totalRequired = component.CountOfGood * goodsItem.Quantity;
                                if (includedGood.Count_In_Availability < totalRequired)
                                {
                                    ModelState.AddModelError("",
                                        $"Недостаточно товара '{includedGood.Name_Of_Good}' в наличии для набора '{good.Name_Of_Good}' " +
                                        $"(требуется: {totalRequired}, доступно: {includedGood.Count_In_Availability})");
                                }
                            }
                        }
                    }
                }

                // Сохраняем заказ
                model.Order.ReceivingData = DateTime.SpecifyKind(model.Order.ReceivingData, DateTimeKind.Unspecified);
                _context.Add(model.Order);
                await _context.SaveChangesAsync();

                // Обрабатываем товары заказа
                foreach (var goodsItem in model.Goods)
                {
                    if (goodsItem.GoodId > 0 && goodsItem.Quantity > 0)
                    {
                        var good = await _context.Goods
                            .Include(g => g.SetContents)
                            .FirstOrDefaultAsync(g => g.Id == goodsItem.GoodId);

                        // Уменьшаем количество для обычного товара
                        if (!good.IsSet)
                        {
                            good.Count_In_Availability -= goodsItem.Quantity;
                            _context.Update(good);
                        }
                        else // Обрабатываем набор
                        {
                            foreach (var component in good.SetContents)
                            {
                                var includedGood = await _context.Goods.FindAsync(component.GoodId);
                                if (includedGood != null)
                                {
                                    includedGood.Count_In_Availability -= component.CountOfGood * goodsItem.Quantity;
                                    _context.Update(includedGood);
                                }
                            }
                        }

                        // Добавляем товар в заказ
                        var orderGoods = new OrderGoods
                        {
                            OrderId = model.Order.Id,
                            GoodId = goodsItem.GoodId,
                            CountOfGood = goodsItem.Quantity
                        };
                        _context.Add(orderGoods);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании заказа: {ex.Message}");
            }

            // Если есть ошибки, возвращаем форму с данными
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", model.Order.CustomerId);
            ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethod, "Id", "PaymentMethodName", model.Order.PaymentMethod);
            ViewData["MethodOfReceiving"] = new SelectList(_context.ReceivingAnOrder, "Id", "TypeOfReceiving", model.Order.MethodOfReceiving);
            ViewData["Goods"] = new SelectList(_context.Goods, "Id", "Name_Of_Good");
            return View(model);
        }
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethodNavigation)
                .Include(o => o.ReceivingAnOrder)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Order == null)
            {
                return Problem("Entity set 'NewAppContext.Order' is null.");
            }

            var order = await _context.Order
                .Include(o => o.OrderGoods)
                    .ThenInclude(og => og.Good)
                        .ThenInclude(g => g.SetContents)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                try
                {
                    // Проверяем, является ли заказ старым (до сегодняшней даты)
                    bool isOldOrder = order.ReceivingData.Date < DateTime.Today;

                    if (!isOldOrder)
                    {
                        // Восстанавливаем количество только для НЕ старых заказов
                        foreach (var orderGood in order.OrderGoods)
                        {
                            if (orderGood.Good != null)
                            {
                                // Восстанавливаем основной товар
                                orderGood.Good.Count_In_Availability += orderGood.CountOfGood;
                                _context.Update(orderGood.Good);

                                // Восстанавливаем компоненты набора (если это набор)
                                if (orderGood.Good.IsSet)
                                {
                                    foreach (var component in orderGood.Good.SetContents)
                                    {
                                        var includedGood = component.Good;
                                        if (includedGood != null)
                                        {
                                            includedGood.Count_In_Availability += component.CountOfGood * orderGood.CountOfGood;
                                            _context.Update(includedGood);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Удаляем связанные товары в заказе (в любом случае)
                    _context.OrderGoods.RemoveRange(order.OrderGoods);

                    // Удаляем сам заказ
                    _context.Order.Remove(order);

                    await _context.SaveChangesAsync();

                    TempData["Message"] = isOldOrder
                        ? "Старый заказ удален (количество товаров не восстановлено)"
                        : "Заказ удален, товары возвращены в наличие";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка при удалении заказа: {ex.Message}");
                    return View("Delete", order);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }
            var order = await _context.Order
                .Include(o => o.OrderGoods)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }
            var viewModel = new EditOrderViewModel
            {
                Order = order,
                OrderGoods = order.OrderGoods.Select(og => new EditOrderGoodsViewModel
                {
                    Id = og.Id,
                    GoodId = og.GoodId,
                    CountOfGood = og.CountOfGood
                }).ToList(),
                AvailableGoods = new SelectList(_context.Goods, "Id", "Name_Of_Good")
            };
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", order.CustomerId);
            ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethod, "Id", "PaymentMethodName", order.PaymentMethod);
            ViewData["MethodOfReceiving"] = new SelectList(_context.ReceivingAnOrder, "Id", "TypeOfReceiving", order.MethodOfReceiving);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, EditOrderViewModel viewModel)
        {
            if (id != viewModel.Order.Id)
            {
                return NotFound();
            }

                try
                {
                    // Получаем текущий заказ с товарами из БД
                    var existingOrder = await _context.Order
                        .Include(o => o.OrderGoods)
                            .ThenInclude(og => og.Good)
                        .FirstOrDefaultAsync(o => o.Id == id);

                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // Обновляем основную информацию о заказе
                    viewModel.Order.ReceivingData = DateTime.SpecifyKind(viewModel.Order.ReceivingData, DateTimeKind.Unspecified);
                    _context.Entry(existingOrder).CurrentValues.SetValues(viewModel.Order);
                    
                    // Обрабатываем товары в заказе
                    var existingOrderGoods = existingOrder.OrderGoods.ToList();

                    // Восстанавливаем количество для удаляемых товаров
                    foreach (var ogVm in viewModel.OrderGoods.Where(og => og.ToDelete && og.Id != 0))
                    {
                        var existingGood = existingOrderGoods.FirstOrDefault(og => og.Id == ogVm.Id);
                        if (existingGood?.Good != null)
                        {
                            existingGood.Good.Count_In_Availability += existingGood.CountOfGood;
                            if (existingGood.Good.IsSet)
                            {
                                var setContents = await _context.SetComposition
                                    .Include(sc => sc.Good)
                                    .Where(sc => sc.SetId == existingGood.Good.Id)
                                    .ToListAsync();

                                foreach (var component in setContents)
                                {
                                    var includedGood = component.Good;
                                    includedGood.Count_In_Availability += component.CountOfGood * existingGood.CountOfGood;
                                    _context.Update(includedGood);
                                }
                            }

                        _context.Update(existingGood.Good);
                            
                        }
                    }

                    // Удаляем помеченные товары
                    var toDelete = viewModel.OrderGoods
                        .Where(og => og.ToDelete && og.Id != 0)
                        .Select(og => og.Id)
                        .ToList();

                    if (toDelete.Any())
                    {
                        var deleteItems = existingOrderGoods.Where(og => toDelete.Contains(og.Id));
                        _context.OrderGoods.RemoveRange(deleteItems);
                    }

                    // Обновляем существующие и добавляем новые товары
                    foreach (var ogVm in viewModel.OrderGoods.Where(og => !og.ToDelete))
                    {
                        if (ogVm.Id == 0) // Новый товар
                        {
                            var good = await _context.Goods.FindAsync(ogVm.GoodId);
                            if (good == null)
                            {
                                ModelState.AddModelError("", $"Товар с ID {ogVm.GoodId} не найден");
                                continue;
                            }

                            // Проверяем достаточное количество
                            if (good.Count_In_Availability < ogVm.CountOfGood)
                            {
                                ModelState.AddModelError("", $"Недостаточно товара {good.Name_Of_Good} в наличии");
                                continue;
                            }

                            // Уменьшаем количество
                            good.Count_In_Availability -= ogVm.CountOfGood;
                            if (good.IsSet)
                            {
                                var setContents = await _context.SetComposition
                                    .Include(sc => sc.Good)
                                    .Where(sc => sc.SetId == good.Id)
                                    .ToListAsync();

                                foreach (var component in setContents)
                                {
                                    var includedGood = component.Good;
                                    int totalRequired = component.CountOfGood * ogVm.CountOfGood;

                                    if (includedGood.Count_In_Availability < totalRequired)
                                    {
                                        ModelState.AddModelError("", $"Недостаточно товара {includedGood.Name_Of_Good} в наличии для набора");
                                        continue;
                                    }

                                    includedGood.Count_In_Availability -= totalRequired;
                                    _context.Update(includedGood);
                                }
                            }

                        _context.Update(good);

                            _context.Add(new OrderGoods
                            {
                                OrderId = viewModel.Order.Id,
                                GoodId = ogVm.GoodId,
                                CountOfGood = ogVm.CountOfGood
                            });
                        }
                        else // Существующий товар
                        {
                            var existing = existingOrderGoods.FirstOrDefault(og => og.Id == ogVm.Id);
                            if (existing != null)
                            {
                                // Восстанавливаем старое количество
                                if (existing.Good != null)
                                {
                                    existing.Good.Count_In_Availability += existing.CountOfGood;
                                    _context.Update(existing.Good);
                                }

                                // Проверяем новый товар и количество
                                var newGood = await _context.Goods.FindAsync(ogVm.GoodId);
                                if (newGood == null)
                                {
                                    ModelState.AddModelError("", $"Товар с ID {ogVm.GoodId} не найден");
                                    continue;
                                }

                                // Проверяем достаточное количество
                                if (newGood.Count_In_Availability < ogVm.CountOfGood)
                                {
                                    ModelState.AddModelError("", $"Недостаточно товара {newGood.Name_Of_Good} в наличии");
                                    continue;
                                }

                                // Уменьшаем новое количество
                                newGood.Count_In_Availability -= ogVm.CountOfGood;
                                _context.Update(newGood);

                                // Обновляем запись
                                existing.GoodId = ogVm.GoodId;
                                existing.CountOfGood = ogVm.CountOfGood;
                                _context.Update(existing);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(viewModel.Order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            // Если есть ошибки, возвращаем форму с данными
            viewModel.AvailableGoods = new SelectList(_context.Goods, "Id", "Name_Of_Good");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", viewModel.Order.CustomerId);
            ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethod, "Id", "PaymentMethodName", viewModel.Order.PaymentMethod);
            ViewData["MethodOfReceiving"] = new SelectList(_context.ReceivingAnOrder, "Id", "TypeOfReceiving", viewModel.Order.MethodOfReceiving);

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(long id)
        {
            return _context.Order.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Order == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Customer)
                    .ThenInclude(c => c.MethodOfCommunication)
                .Include(o => o.PaymentMethodNavigation)
                .Include(o => o.ReceivingAnOrder)
                .Include(o => o.OrderGoods)
                    .ThenInclude(og => og.Good)
                        .ThenInclude(g => g.Category)
                .Include(o => o.OrderGoods)
                    .ThenInclude(og => og.Good)
                        .ThenInclude(g => g.Type)
                .Include(o => o.OrderGoods)
                    .ThenInclude(og => og.Good)
                        .ThenInclude(g => g.Material)
                .Include(o => o.OrderGoods)
                    .ThenInclude(og => og.Good)
                        .ThenInclude(g => g.SetContents)
                            .ThenInclude(sc => sc.Good)
                                .ThenInclude(g => g.Category)
                .Include(o => o.OrderGoods)
                    .ThenInclude(og => og.Good)
                        .ThenInclude(g => g.SetContents)
                            .ThenInclude(sc => sc.Good)
                                .ThenInclude(g => g.Type)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
