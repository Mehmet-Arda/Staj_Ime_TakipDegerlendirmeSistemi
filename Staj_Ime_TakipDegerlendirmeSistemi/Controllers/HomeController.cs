using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Staj_Ime_TakipDegerlendirmeSistemi.Controllers
{
    
    public class HomeController : Controller
    {

        Entities1 db = new Entities1();

        public ActionResult Index()
        {
            return View();
        }
       
        
    }
}