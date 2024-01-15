using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.Identity;
using ProductionWebService.Helpers;

namespace ProductionWebService.Security
{
    public class OutboundServicesAuthorizeAttribute : UserIsValidAuthorizationAttribute//: AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext context)
        {
            String IPAddress = IPAddressHelper.GetIPAddress();
            int? UserId = RequestHelper.GetUserId(context); // context.RequestContext.Principal.Identity.GetUserId();
            String RouteUrl = String.Format("/{0}/", RouteHelper.GetControllActionRoute(context));

            if (base.IsAuthorized(context) && ServiceAccessManager.HasAccess(UserId, RouteUrl, IPAddress))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}