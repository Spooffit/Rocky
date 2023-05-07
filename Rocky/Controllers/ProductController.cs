using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System.Data;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;

        public ProductController(IProductRepository productRepository, IWebHostEnvironment env)
        {
            _productRepository = productRepository;
            _env = env;
        }

        public IActionResult Index()
        {
            var productList = _productRepository.GetAll(includeProperties: "Category,ApplicationType");

            return View(productList);
        }

        //GET - UPSERT
        public IActionResult Upsert(int? id) 
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _productRepository.GetAllDropdownList(WC.CategoryName),
                ApplicationTypeSelectList = _productRepository.GetAllDropdownList(WC.ApplicationTypeName)
            };

            if (id == null) // CREATE
                return View(productVM);
            else // EDIT
            {
                productVM.Product = _productRepository.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                    return NotFound();
                else
                    return View(productVM);
            }
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid) 
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _env.WebRootPath;

                if (productVM.Product.Id == 0) //CREATE
                {
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                        files[0].CopyTo(fileStream);

                    productVM.Product.Image = fileName + extension;

                    _productRepository.Add(productVM.Product);
                }
                else //UPDATE
                {
                    var objFromDb = _productRepository.FirstOrDefault(x => x.Id == productVM.Product.Id, IsTracking:false);

                    if (files.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                                System.IO.File.Delete(oldFile);

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                            files[0].CopyTo(fileStream);

                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _productRepository.Update(productVM.Product);
                } 

                _productRepository.Save();
                TempData[WC.Success] = "Action has been performed successfully";
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _productRepository.GetAllDropdownList(WC.CategoryName);
            productVM.ApplicationTypeSelectList = _productRepository.GetAllDropdownList(WC.ApplicationTypeName);
            TempData[WC.Error] = "Error while performing action";
            return View(productVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var obj = _productRepository.FirstOrDefault(u => u.Id == id,includeProperties: "Category,ApplicationType");

            if (obj == null)
                return NotFound(); 

            return View(obj);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _productRepository.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                TempData[WC.Error] = "Error while deleting Product";
                return RedirectToAction("Index");
            }

            var filePath = Path.Combine(_env.WebRootPath + WC.ImagePath + obj.Image);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _productRepository.Remove(obj);

            _productRepository.Save();
            TempData[WC.Success] = "Product has been deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
