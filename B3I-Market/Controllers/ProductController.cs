using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B3I_Market.Helpers;
using BLL;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace B3I_Market.Controllers
{
    public class ProductController : Controller
    {
        public readonly int Qty = 5;
        private IWebHostEnvironment _environment = null;

        public ProductController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
    
        public IActionResult CategoryProducts(string category)
        {
            ClearHttp();
            var filters = new ProductFiltersViewModel();
            filters.Category = category;
            string inputString = Request.Query.FirstOrDefault(p => p.Key == "inputSearch").Value;
            filters.inputSearch = inputString;
            var model = CategoryProductLogic.GetProductsWithFilters(filters,0,Qty,inputString);
            
            if (model.GetModel != null)
            {
                return View(model.GetModel);
            }
            TempData.AddOrUpdate("Error", model.GetErrorMessage);
            

            return Redirect(Request.Headers["Referer"].ToString());
        }
        [HttpPost]
        public PartialViewResult GetMore(string category, int offset,string inputSearch)
        {
            
            ProductLogic.ModelOrError<OutputModelsForCategoryProduct.CategoryProductsModel> model;
            ProductFiltersViewModel filters = new ProductFiltersViewModel {Category = category, inputSearch = inputSearch};
            if (HttpContext.Session.Keys.Contains("ProductFilters"))
            {
                filters = HttpContext.Session.Get<ProductFiltersViewModel>("ProductFilters");
            }
            model = CategoryProductLogic.GetProductsWithFilters(filters, offset, Qty, inputSearch);
            if (model.GetModel != null) return PartialView("ProductCard",  model.GetModel);
            return null;
        }
        [HttpPost]
        public PartialViewResult Filter(ProductFiltersViewModel model)
        {
            ClearHttp();
            HttpContext.Session.SetOrUpdate<ProductFiltersViewModel>("ProductFilters", model);
            var result = CategoryProductLogic.GetProductsWithFilters(model, 0, Qty, model.inputSearch);

            return PartialView("ProductCard", result.GetModel);
        }

        private void ClearHttp()
        {
            if (HttpContext.Session.Keys.Contains("ProductFilters"))
            {
                HttpContext.Session.Remove("ProductFilters");
            }
        }

        public IActionResult Index(string id)
        {
            var model = BLL.ProductLogic.GetProductView(id);
            if (model.GetModel != null) return View(model.GetModel);
            TempData.AddOrUpdate("Error", model.GetErrorMessage);
            return Redirect(Request.Headers["Referer"].ToString());

        }

        public IActionResult Categories()
        {
            var model = CategoriesLogic.GetListOfCategoriesNames.Invoke(null);
            return View(model);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult NewCategory()
        {
            return View();
        }
        
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> NewCategory(Category model, IFormFile picture)
        {
            string photoPath;
            try
            {
                if (picture != null)
                {
                    photoPath = await _environment.AddFile(picture, "/img/");
                    model.PhotoPath = photoPath;
                }
            }
            catch(Exception e)
            {
                TempData.AddOrUpdate("CategoryError", e.Message);
                return View("NewCategory");
            }
            var message = CategoriesLogic.AddCategory(model);
            if(message.GetErrorMessage!=null) TempData.AddOrUpdate("CategoryError", message.GetErrorMessage);
            return View("NewCategory", message.GetModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteCategory(string categoryName)
        {
            var result = CategoriesLogic.DeleteCategory(categoryName);
            if(result.GetErrorMessage == null)
            {
                if(result.GetModel != null )
                {
                    _environment.DeletePhoto(result.GetModel.PhotoPath);
                    TempData["CategoryDeletMessage"] = "Category deleted with photo";
                    return RedirectToAction("Categories", "Product");
                }
                else
                {
                    TempData["CategoryDeletMessage"] = "Category deleted without photo";
                    return RedirectToAction("Categories", "Product");
                }
            }
            TempData["CategoryDeletMessage"] = result.GetErrorMessage;
            return RedirectToAction("Categories", "Product");
        }
        
        
        [Authorize(Roles = "Admin")]
        public IActionResult NewProduct()
        {
            SetCategoriesInViewData();
            return View();
        }

        private void SetCategoriesInViewData()
        {
            var categories = BLL.CategoriesLogic.GetListOfCategoriesNames
                .Invoke(null)
                .Select(p => p.Name)
                .ToList();
            ViewData.Add("Categories", categories);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> NewProduct(Product product, IFormFile photo)
        {
            string photoPath;
                try
                {
                    if (photo != null)
                    {
                        photoPath = await _environment.AddFile(photo, "/img/");
                        product.PhotoPath = photoPath;
                    }
                }
                catch
                {
                    ModelState.AddModelError("PhotoPath", "Can't handle photo=(");
                }

                TempData.AddOrUpdate("Success", BLL.ProductLogic.InsertProduct(product));
                SetCategoriesInViewData();
                return View(product);     
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult EditProduct(string _id)
        {
            var product = ProductLogic.GetProductById(_id);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditProductForm(Product product, IFormFile photo,string _id,string _photoPath)
        {
            string photoPath;
            product.PhotoPath = _photoPath;
            try
            {
                if (photo != null)
                {
                    photoPath = await _environment.AddFile(photo, "/img/");
                    if (photoPath != _photoPath && _photoPath!="no-image.png")
                        if(!BLL.ProductLogic.IFthisPhoroUseMoreProduct(_photoPath))
                            _environment.DeletePhoto(_photoPath);
                    product.PhotoPath = photoPath;
                }
            }
            catch
            {
                ModelState.AddModelError("PhotoPath", "Can't handle photo=(");
            }
            TempData.Add("Success", BLL.ProductLogic.InsertProductByID(product, _id));
            return RedirectToAction("EditProduct", new { _id = _id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult GetCharacteristics(string categoryName)
        {
            var result = BLL.CategoriesLogic.GetCharacteristics(categoryName);
            if (result.GetErrorMessage != null)
            {
                TempData.AddOrUpdate("CharacteristicError", result.GetErrorMessage);
            }
            return new JsonResult(result.GetModel);
        }
        [HttpPost]
        public string AddToCard(string productId)
        {
            var card = HttpContext.Session.Get<Dictionary<string, int>>("Card");
            var result = BLL.ProductLogic.ValidateAddToCard(productId, card);
            HttpContext.Session.SetOrUpdate<Dictionary<string, int>>("Card", card);
            return result;
        }



        [HttpPost]
        [Authorize]
        [ActionName("Index")]
        public IActionResult Comment(ReviewViewModel model, string productId)
        {
            model.Product = productId;
            model.UserName = User.Identity.Name;
            var comment = ProductLogic.AddComment(model);
            if(comment.GetErrorMessage!=null) TempData.AddOrUpdate("CommentError", comment.GetErrorMessage);
            return Index(productId);
            
        }
        [HttpGet]
        public IActionResult Search(string inputSearch)
        {
            List<Category> _categorys = BLL.CategoriesLogic.GetSearchOfCategory(inputSearch);
            if (_categorys.Count==0)
            {
                TempData.AddOrUpdate("SearchError", "Not Found");
                return View("Categories", _categorys);
            }
            return View("Categories", _categorys);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProduct(string _id)
        {
            var result = ProductLogic.DeleteProduct(_id);
            if (result.GetErrorMessage == null)
            {
                if (result.GetModel != null && !BLL.ProductLogic.IFthisPhoroUseMoreProduct(result.GetModel.PhotoPath))
                {
                    _environment.DeletePhoto(result.GetModel.PhotoPath);
                    TempData["ProductDeletMessage"] = "Product deleted with photo";
                    return RedirectToAction("Categories", "Product");
                }
                else
                {
                    TempData["ProductDeletMessage"] = "Product deleted without photo";
                    return RedirectToAction("Categories", "Product");
                }
            }
            TempData["ProductDeletMessage"] = result.GetErrorMessage;
            return RedirectToAction("Categories", "Product");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteComment(int id, string product)
        {
            var result = BLL.DeleteComment.Do(id);
            TempData.AddOrUpdate("DeleteStatus", result);
            return RedirectToAction("Index", "Product", new{id = product});
        }
    }
}