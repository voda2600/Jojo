using System.Collections.Generic;
using B3I_Market.Helpers;
using B3I_Market.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace B3I_Market.Controllers
{
    public class OrderController : Controller
    {
        public IPaymenetService _paymenet;
        public OrderController(IPaymenetService paymenet)
        {
            _paymenet = paymenet;
        }
        [Authorize(Roles = "User")]
        public IActionResult Card()
        {
            var ids = HttpContext.Session.Get<Dictionary<string, int>>("Card");
            var products = BLL.Card.GetCard(ids);
            if (products.GetModel != null)
            {
                return View(products.GetModel);
            }
            else
            {
                TempData.AddOrUpdate("EmptyCardError", products.GetErrorMessage);
                return View();
            }
            
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Info(int id)
        {
            var model = BLL.OrderLogic.GetOrderRecordBy(id);
            if (model.GetModel == null)
            {
                TempData.Add("Error", model.GetErrorMessage);
                return RedirectToAction("Index", "Account");
            }
            else
            {
                return View(model.GetModel);
            }
            
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Info(int id, string status)
        {
            string scheme = Url.ActionContext.HttpContext.Request.Scheme;
            string url = Url.Action("OrderTimeline", "Order", new{id=id}, scheme);
            var result = BLL.OrderLogic.ChangeStatus(id, status, url);
            TempData.Add("UpdateStatusResult", result);
            return Info(id);
        }

        public IActionResult OrderTimeline(int id)
        {
            var model = BLL.OrderLogic.GetTimeLine(id, User);
            if (model.GetErrorMessage != null)
            {
                TempData.Add("Error", model.GetErrorMessage);
                return RedirectToAction("Index", "Account");
            }
            return View(model.GetModel);
        }

        public IActionResult PaymentInfo(OrderConfirmViewModel model)
        {
            var ids = HttpContext.Session.Get<Dictionary<string, int>>("Card");
            var unavaliableProds = BLL.OrderLogic.GetUnavaliableProducts(ids);
            if (unavaliableProds.Count != 0)
            {
                foreach (var unavaliableProd in unavaliableProds)
                    ids.Remove(unavaliableProd);
                HttpContext.Session.SetOrUpdate("Card", ids);
                TempData["UnavaliableProdsMessage"] = "There were unavailable items in your shopping cart. They are removed from the card.";
                return RedirectToAction("Card", "Order");
            }
            return View(model);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult CreateOrderFail()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult PaymentInfoAcceptance(OrderConfirmViewModel model)
        {
            var ids = HttpContext.Session.Get<Dictionary<string, int>>("Card");
            var unavaliableProds = BLL.OrderLogic.GetUnavaliableProducts(ids);
            if(unavaliableProds.Count != 0)
            {
                foreach (var unavaliableProd in unavaliableProds)
                    ids.Remove(unavaliableProd);
                HttpContext.Session.SetOrUpdate("Card", ids);
                TempData["UnavaliableProdsMessage"] = "There were unavailable items in your shopping cart. They are removed from the card.";
                return RedirectToAction("Card", "Order");
            }
            var result = BLL.OrderLogic.CreateOrder(User.Identity.Name, model.Address, model.DelivetyType, ids, model.CardMonth, model.CardYear);
            result = "DDD";
            if (result == "Ok")
            {
                _paymenet.Pay();
                HttpContext.Session.SetOrUpdate("Card", new Dictionary<string, int>());
                return RedirectToAction("Success", "Order");
            }
            if (result == "Card is not avaliable")
            {
                TempData["CardUnavaliable"] = "Card is not avaliable";
                return RedirectToAction("PaymentInfo", "Order", model);
            }
            else
            {
                return RedirectToAction("CreateOrderFail", "Order");
            }
        }
        
        public IActionResult RemoveFromCard(string productId)
        {
            HttpContext.Session.Remove(productId);
            var temp = HttpContext.Session.Get<Dictionary<string, int>>("Card");
            var delete = temp.Remove(productId);
            if (delete)
            {
                HttpContext.Session.SetOrUpdate("Card", temp);
                return RedirectToAction("Card", "Order");
            }
            TempData.Add("RemoveFromCardError", "Can't delete product from card");
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public JsonResult QtyChange(string productId, int qty)
        {
            var card = HttpContext.Session.Get<Dictionary<string, int>>("Card");
            card[productId] = qty;
            HttpContext.Session.SetOrUpdate<Dictionary<string, int>>("Card", card);
            string msg = "Ok";
            return Json(msg);
        }
    }
}
