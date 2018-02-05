using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace phaBalloting.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "admin" });
            }

            return RedirectToAction("Login", "Account", new { area = "" });
        }
        
    }
}