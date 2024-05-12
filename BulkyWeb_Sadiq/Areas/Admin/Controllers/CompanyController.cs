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
	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		
		public CompanyController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
			
		}
		public IActionResult Index()
		{
			var ObjCompanyList = _unitOfWork.Company.GetAll().ToList();
			
			return View(ObjCompanyList);
		}
		
		public IActionResult Upsert(int? Id)
		{
			
			if (Id == null || Id == 0)
			{
				return View(new Company());
			}
			else
			{
				Company companyObj = _unitOfWork.Company.Get(u => u.Id == Id);
				return View(companyObj);
			}
			
		}

		[HttpPost]
		public IActionResult Upsert(Company CompanyObj)
		{
			if (ModelState.IsValid)
			{
				
				if (CompanyObj.Id == 0)
				{
					_unitOfWork.Company.Add(CompanyObj);
				}
				else
				{
					_unitOfWork.Company.update(CompanyObj);
				}
				
				_unitOfWork.Save();
				TempData["success"] = "Company created Succesfully";
				return RedirectToAction("Index", "Company");
			}
			else
			{
				
			}
			return View(CompanyObj);
		}

		

		
		#region API CALLS
		[HttpGet]
		public IActionResult GetAll() 
		{ 
		   var objCompanyList= _unitOfWork.Company.GetAll().ToList();
			return Json(new { data = objCompanyList });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var CompanyToBeDeleted= _unitOfWork.Company.Get(u=>u.Id == id);
			if(CompanyToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			
			_unitOfWork.Company.Remove(CompanyToBeDeleted);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Company deleted succesfully" });
			
		}

		#endregion
	}
}
