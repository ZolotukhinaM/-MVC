using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Курсовая_работа_MVC.Models;

namespace Курсовая_работа_MVC.Controllers.Customers
{
    public class CustomerController : Controller
    {
        private readonly NewAppContext _context;

        public CustomerController(NewAppContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string name = "", string surname = "")
        {
            IQueryable<Customer> query = _context.Customers
                .Include(c => c.MethodOfCommunication);

            // Фильтрация по имени (если указано)
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            // Фильтрация по фамилии (если указано)
            if (!string.IsNullOrEmpty(surname))
            {
                query = query.Where(c => c.Surname != null && c.Surname.Contains(surname));
            }

            // Сохраняем параметры поиска
            ViewBag.Name = name;
            ViewBag.Surname = surname;
            ViewBag.Ways = _context.WaysOfConnect.ToList();

            return View(query.ToList());
        }
        [HttpGet]
        public IActionResult GetCustomers()
        {
            var customers = _context.Customers
                .Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    surname = c.Surname
                })
                .ToList();
            return Json(customers);
        }
        [HttpGet]
        public IActionResult Create(string source = null, long? orderId = null)
        {

            ViewBag.Ways = _context.WaysOfConnect.ToList();
            ViewBag.Source = source; 
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Customer customer, string source = null, long? orderId = null)
        {
            if (ModelState.IsValid)
            {
                // Проверка: есть ли уже такой номер телефона
                if (_context.Customers.Any(c => c.PhoneNumber == customer.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Покупатель с таким номером телефона уже существует.");
                }
                else
                {
                    _context.Customers.Add(customer);
                    _context.SaveChanges();

                    if (source == "order")
                    {
                        return RedirectToAction("Create", "Orders", new
                        {
                            customerAdded = true,
                            newCustomerId = customer.Id
                        });
                    }

                    return RedirectToAction("Index", "Customer");
                }
            }
            ViewBag.Ways = _context.WaysOfConnect.ToList();
            ViewBag.Source = source;
            ViewBag.OrderId = orderId;

            return View(customer);
        }


        [HttpGet]
        public IActionResult Edit(long id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null) return NotFound();
            ViewBag.Ways = _context.WaysOfConnect.ToList();
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Проверка: нет ли другого покупателя с таким же номером
                if (_context.Customers.Any(c => c.PhoneNumber == customer.PhoneNumber && c.Id != customer.Id))
                {
                    ModelState.AddModelError("PhoneNumber", "Покупатель с таким номером телефона уже существует.");
                }
                else
                {
                    _context.Customers.Update(customer);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Ways = _context.WaysOfConnect.ToList();
            return View(customer);
        }


        [HttpPost]
        public IActionResult Delete(long id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
