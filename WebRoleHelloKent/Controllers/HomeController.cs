using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRoleHelloKent.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Hello Kent";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "KENT.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "KENT";

            return View();
        }
    }
}
