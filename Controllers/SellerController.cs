using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Курсовая_работа_MVC.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}