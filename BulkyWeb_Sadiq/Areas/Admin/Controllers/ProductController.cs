using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace BulkyWeb_Sadiq.Areas.Admin.Controllers
{
    //[Authorize(Roles = SD.Role_Admin)]
    [Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
		public IActionResult Index()
		{
			var ObjProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
			
			return View(ObjProductList);
		}
		
		public IActionResult Upsert(int? Id)
		{
			ProductVM productVm = new ProductVM()
			{
				CategoryList= _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				}),
				Product= new Product()

		    };
			if (Id == null || Id == 0)
			{
				return View(productVm);
			}
			else
			{
				productVm.Product = _unitOfWork.Product.Get(u => u.Id == Id);
				return View(productVm);
			}
			
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVm, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName= Guid.NewGuid().ToString() +Path.GetExtension(file.FileName);
					string productPath= Path.Combine(wwwRootPath,@"images\product");
					if(!string.IsNullOrEmpty(productVm.Product.ImageUrl)) 
					{
						//Delete old image
						var oldImagePath= Path.Combine(wwwRootPath,productVm.Product.ImageUrl.TrimStart('\\'));
						if(System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}
					productVm.Product.ImageUrl = @"\images\product\" + fileName;
				}
				if (productVm.Product.Id == 0)
				{
					_unitOfWork.Product.Add(productVm.Product);
				}
				else
				{
					_unitOfWork.Product.Update(productVm.Product);
				}
				
				_unitOfWork.Save();
				TempData["success"] = "Product created Succesfully";
				return RedirectToAction("Index", "Product");
			}
			else
			{
				productVm.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				});
			}
			return View(productVm);
		}

		

		
		#region API CALLS
		[HttpGet]
		public IActionResult GetAll() 
		{ 
		   var objProductList= _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = objProductList });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productToBeDeleted= _unitOfWork.Product.Get(u=>u.Id == id);
			if(productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			//Delete old image
			var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}
			_unitOfWork.Product.Remove(productToBeDeleted);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Product deleted succesfully" });
			
		}

		#endregion
	}
}
