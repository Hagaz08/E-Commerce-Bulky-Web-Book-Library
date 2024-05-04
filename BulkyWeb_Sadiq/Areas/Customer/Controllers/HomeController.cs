using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb_Sadiq.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }

		[HttpGet]
		public IActionResult Details(int productId)
		{


            var Cart = new ShoppingCart
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId

            };


            return View(Cart);
		}

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId= UserId;
          
            ShoppingCart cartFromDb= _unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId== UserId && u.ProductId==shoppingCart.ProductId);
            if (cartFromDb != null)
            {
                //cart already exist for the product and by the user, so update
                cartFromDb.Count+=shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                TempData["success"] = "Cart Updated Succesfully";
            }
            else
            {
                //cart does not exist, so add cart to db
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                TempData["success"] = "Cart added Succesfully";
            }
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}