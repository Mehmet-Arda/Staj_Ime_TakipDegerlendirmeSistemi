using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Filters
{
    public class TeacherAuthFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["teacher"] == null)
            {
                filterContext.Result = new RedirectResult("/Teacher/Login");
            }

        }
    }
}