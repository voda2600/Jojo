using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using B3I_Market.Helpers;
using BLL;
using DAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ViewModels;

namespace B3I_Market.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly int Qty = 5;

        

        private ProfileViewModel GetDefaultModel()
        {
            var model = new ProfileViewModel {Page = 1, Qty = this.Qty};
            return model;
        }
        [Authorize]
        public IActionResult Index(ProfileViewModel model, int page = 1, bool changeFilters = true)
        {
            if (changeFilters)
            {
                HttpContext.Session.SetOrUpdate<ProfileViewModel>("ProfileFilters", model);
            }
            else
            {
                model = HttpContext.Session.Get<ProfileViewModel>("ProfileFilters");
            }
            if (User.IsInRole("User"))
            {
                model.UserName = User.Identity.Name;
            }
            if (model.Status == "-") model.Status = null;
            if (model.SortType == "-") model.SortType = null;
            model.Page = page;
            model.Qty = Qty;
            var result = ProfileLogic.GetProfile(model);
            if(result.GetModel!=null) return View(result.GetModel);
            TempData.AddOrUpdate("Error", result.GetErrorMessage);
            return Redirect(Request.Headers["Referer"].ToString());
            
        }
        [Authorize(Roles = "User")]
        public IActionResult Edit()
        {
            return View();
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult EditPassword(EditPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                model.UserName = User.Identity.Name;
                
                var result = ProfileLogic.EditPassword(model);
                TempData.AddOrUpdate("EditPasswordMessage", result);
                return RedirectToAction("Edit", "Account");
            }
            if (ModelState["ConfirmPassword"].ValidationState == ModelValidationState.Invalid)
            {
                ModelState.AddModelError(string.Empty, "Passwords don't match");
            }
            return RedirectToAction("Edit", "Account");
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult EditInfo(EditInfoViewModel model)
        {
            if(ModelState.IsValid)
            {
                model.UserName = User.Identity.Name;
                var result = ProfileLogic.EditMail(model);
                TempData.AddOrUpdate("EditMailMessage", result);
                return RedirectToAction("Edit", "Account");
            }
            TempData.Add("EditMailMessage", "Email is not valid");
            return RedirectToAction("Edit", "Account");
        }   
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            var user = RegisterLogic.Register(model);
            
            if (user.GetUser==null)
            {
                TempData.AddOrUpdate("SignUpError", user.GetErrorMessage);
            }
            else
            {
                await Authenticate(user.GetUser);
            }

            
            return Redirect(Request.Headers["Referer"].ToString());
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(LogInViewModel model)
        {
            var user = LogInLogic.GetUser(model);
            if (user == null)
            {
                TempData.AddOrUpdate("LogInError", "Username or password is not correct!");
            }
            else
            {
                await Authenticate(user);
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
        
        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            string referer = Request.Headers["Referer"].ToString();
            HttpContext.Session.Clear();
            return Redirect(referer);
        }
    }
}
