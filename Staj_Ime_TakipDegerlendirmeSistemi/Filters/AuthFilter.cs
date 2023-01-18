using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Filters
{
    public class AuthFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["admin"]==null)
            {
                filterContext.Result = new RedirectResult("/Admin/Login");
            }
            
        }
    }
}