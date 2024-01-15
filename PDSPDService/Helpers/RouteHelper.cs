using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web;
using System.Web.Http.Controllers;

namespace ProductionWebService.Helpers
{
    public class RouteHelper
    {
        public static string GetControllActionRoute(HttpActionContext context)
        {
            return String.Format("{0}/{1}", context?.ActionDescriptor?.ControllerDescriptor.ControllerName, context?.ActionDescriptor?.ActionName);
        }
    }
}