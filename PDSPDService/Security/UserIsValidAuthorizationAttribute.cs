using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using ProductionWebService.Providers;
using ProductionWebService.Helpers;

namespace ProductionWebService.Security
{
    public class UserIsValidAuthorizationAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext context)
        {
            var IsAuthorized = false;
            var UserId = RequestHelper.GetUserId(context);

            if (UserId != null)
            {
                if (base.IsAuthorized(context) && UserAccessManager.HasAccess(UserId))
                {
                    IsAuthorized = true;
                }
                else
                {
                    IsAuthorized = false;
                }
            }
            else
            {
                IsAuthorized = false;
            }
            

            return IsAuthorized;
        }
    }
}