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
    [Authorize(Roles = SD.Role_Admin)]
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
				productVm.Product = _unitOfWork.Product.Get(u => u.Id == Id,includeProperties:"ProductImages");
				return View(productVm);
			}
			
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVm, List<IFormFile>? files)
		{
			if (ModelState.IsValid)
			{
				if (productVm.Product.Id == 0)
				{
					_unitOfWork.Product.Add(productVm.Product);
				}
				else
				{
					_unitOfWork.Product.Update(productVm.Product);
				}

				_unitOfWork.Save();
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (files != null)
				{
					foreach(IFormFile file in files)
					{
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						string productPath = @"images\products\product-"+productVm.Product.Id;
						string finalPath= Path.Combine(wwwRootPath,productPath);
						if(!Directory.Exists(finalPath))
						{
							Directory.CreateDirectory(finalPath);
						}
						using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
						{
							file.CopyTo(fileStream);
						}
						ProductImage productImage = new()
						{
							ImageUrl = @"\" + productPath + @"\" + fileName,
							ProductId = productVm.Product.Id,
						};
						if(productVm.Product.ProductImages==null)
						{
							productVm.Product.ProductImages = new List<ProductImage>();
							
						}
						productVm.Product.ProductImages.Add(productImage);
					}
					_unitOfWork.Product.Update(productVm.Product);
					_unitOfWork.Save();
				}
				
				TempData["success"] = "Product created/updated Succesfully";
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

		public IActionResult DeleteImage(int imageId)
		{
			var imageToBeDeleted=_unitOfWork.ProductImage.Get(u=>u.Id== imageId);
			int productId= imageToBeDeleted.ProductId;
			if (imageToBeDeleted != null)
			{
				if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
				{
					var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}
				}
				_unitOfWork.ProductImage.Remove(imageToBeDeleted);
				_unitOfWork.Save();
				TempData["success"] = "Deleted succesfully";
			}
			return RedirectToAction(nameof(Upsert), new { id = productId });
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
			
			string productPath = @"images\products\product-" + id;
			string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);
			if (Directory.Exists(finalPath))
			{
				string[] filePaths= Directory.GetFiles(finalPath);
				foreach (string filePath in filePaths)
				{
					System.IO.File.Delete(filePath);
				}
				Directory.Delete(finalPath);
			}
			_unitOfWork.Product.Remove(productToBeDeleted);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Product deleted succesfully" });
			
		}

		#endregion
	}
}
