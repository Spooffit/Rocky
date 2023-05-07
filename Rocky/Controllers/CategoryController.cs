using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRep)
        {
            _categoryRepository = categoryRep;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objList = _categoryRepository.GetAll();
            return View(objList);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Add(obj);
                _categoryRepository.Save();
                TempData[WC.Success] = "Category has been created successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Error while creating Category";
            return View(obj);
        }


        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var obj = _categoryRepository.Find(id.GetValueOrDefault());
            if (obj == null)
                return NotFound();

            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Update(obj);
                _categoryRepository.Save();
                TempData[WC.Success] = "Category has been updated successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Error while updating Category";
            return View(obj);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var obj = _categoryRepository.Find(id.GetValueOrDefault());
            if (obj == null)
                return NotFound();

            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _categoryRepository.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                TempData[WC.Error] = "Error while deleting Category";
                return RedirectToAction("Index");
            }

            _categoryRepository.Remove(obj);
            _categoryRepository.Save();
            TempData[WC.Success] = "Category has been deleted successfully";
            return RedirectToAction("Index");
        }

    }
}
