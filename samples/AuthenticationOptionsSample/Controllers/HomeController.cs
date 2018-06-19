﻿using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthenticationOptionsSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var tc = HttpContext.GetTenantContext();
            var title = (tc?.Name ?? "No tenant") + " - ";

            ViewData["style"] = "navbar-light bg-light";

            if (!User.Identity.IsAuthenticated)
            {
                title += "Not Authenticated";
            }
            else
            {
                title += "Authenticated";
                ViewData["style"] = "navbar-dark bg-dark";
            }

            ViewData["Title"] = title;

            if (tc != null)
            {
                var cookieOptionsMonitor = HttpContext.RequestServices.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
                var cookieName = cookieOptionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name;
                ViewData["CookieName"] = cookieName;

                var schemes = HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                ViewData["ChallengeScheme"] = schemes.GetDefaultChallengeSchemeAsync().Result.Name;
            }

            return View(tc);
        }

        [Authorize]
        public IActionResult Authenticate([FromServices] IAuthorizationService auth)
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync();
            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Username") }, "Cookies")));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}