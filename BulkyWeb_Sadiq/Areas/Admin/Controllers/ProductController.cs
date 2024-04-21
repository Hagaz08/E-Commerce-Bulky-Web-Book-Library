using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb_Sadiq.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public ProductController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var ObjProductList = _unitOfWork.Product.GetAll().ToList();
			
			return View(ObjProductList);
		}

		public IActionResult Create()
		{
			IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
			{
				Text = u.Name,
				Value = u.Id.ToString(),
			});
			ViewBag.CategoryList = CategoryList;	
			return View();
		}
		[HttpPost]
		public IActionResult Create(Product obj)
		{
			if (ModelState.IsValid)
			{
				_unitOfWork.Product.Add(obj);
				_unitOfWork.Save();
				TempData["success"] = "Product created Succesfully";
				return RedirectToAction("Index", "Product");
			}
			return View();
		}

		public IActionResult Edit(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Product? productfromDb = _unitOfWork.Product.Get(u => u.Id == id);
			if (productfromDb == null)
			{
				return NotFound();
			}
			return View(productfromDb);
		}
		[HttpPost]
		public IActionResult Edit(Product obj)
		{
			if (ModelState.IsValid)
			{
				_unitOfWork.Product.update(obj);
				_unitOfWork.Save();
				TempData["success"] = "Product updated Succesfully";
				return RedirectToAction("Index", "Product");
			}
			return View();
		}

		public IActionResult Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Product? productfromDb = _unitOfWork.Product.Get(u => u.Id == id);
			if (productfromDb == null)
			{
				return NotFound();
			}
			return View(productfromDb);
		}

		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePost(int? id)
		{
			Product? productfromDb = _unitOfWork.Product.Get(u => u.Id == id);
			if (productfromDb == null)
			{
				return NotFound();
			}
			_unitOfWork.Product.Remove(productfromDb);
			_unitOfWork.Save();
			TempData["success"] = "Product deleted Succesfully";
			return RedirectToAction("Index", "Product");

		}
	}
}
