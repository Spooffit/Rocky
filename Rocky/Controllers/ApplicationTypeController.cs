using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;
using System.Data;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        private readonly IApplicationTypeRepository _appTypeRepository;

        public ApplicationTypeController(IApplicationTypeRepository appTypeRepository)
        {
            _appTypeRepository = appTypeRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<ApplicationType> objList = _appTypeRepository.GetAll();
            return View(objList);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(ApplicationType obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _appTypeRepository.Add(obj);
        //        _appTypeRepository.Save();
        //        TempData[WC.Success] = "Application Type has been created successfully";
        //        return RedirectToAction("Index");
        //    }
        //    TempData[WC.Error] = "Error while creating Application Type";
        //    return View(obj);
        //}


        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var obj = _appTypeRepository.Find(id.GetValueOrDefault());
            if (obj == null)
                return NotFound();

            return View(obj);
        }

        //POST - EDIT

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Edit(ApplicationType obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _appTypeRepository.Update(obj);
        //        _appTypeRepository.Save();
        //        TempData[WC.Success] = "Application Type has been updated successfully";
        //        return RedirectToAction("Index");
        //    }
        //    TempData[WC.Error] = "Error while updating Application Type";
        //    return View(obj);
        //}

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var obj = _appTypeRepository.Find(id.GetValueOrDefault());
            if (obj == null)
                return NotFound();

            return View(obj);
        }

        //POST - DELETE

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult DeletePost(int? id)
        //{
        //    var obj = _appTypeRepository.Find(id.GetValueOrDefault());
        //    if (obj == null)
        //    {
        //        TempData[WC.Error] = "Error while deleting Application Type";
        //        return RedirectToAction("Index");
        //    }

        //    _appTypeRepository.Remove(obj);
        //    _appTypeRepository.Save();
        //    TempData[WC.Success] = "Application Type has been deleted successfully";
        //    return RedirectToAction("Index");
        //}
    }
}
