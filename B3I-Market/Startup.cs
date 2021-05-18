using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DAL.Models;
using BLL;
using B3I_Market.Helpers;
using B3I_Market.Services;
using System.Linq;

namespace B3I_Market
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddSessionStateTempDataProvider();
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPaymenetService, PaymentService>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => 
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/");
                    options.AccessDeniedPath = new PathString("/");
                });
            services.AddControllersWithViews();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication(); 
            app.UseAuthorization();

            app.Use(async(context, next) =>

            {
                if (!context.Session.Keys.Contains("statesList"))
                {
                    List<State> states = CityLogic.GetListOfStates.Invoke(null);
                    context.Session.SetOrUpdate<IEnumerable<State>>("statesList", states);
                }
                if (!context.Session.Keys.Contains("citiesList"))
                {
                    List<City> cities = CityLogic.GetListOfCities.Invoke(null);
                    context.Session.SetOrUpdate<IEnumerable<City>>("citiesList", cities);
                }
                await next.Invoke();
            });
            app.Use(async (context, next) =>
            {
                if (context.User.IsInRole("User") && !context.Session.Keys.Contains("Card"))
                {
                    context.Session.SetOrUpdate<Dictionary<string, int>>("Card", new Dictionary<string, int>());   
                }
                await next.Invoke();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}