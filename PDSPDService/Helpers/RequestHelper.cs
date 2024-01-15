using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using ProductionWebService.Providers;
using System.Security.Principal;
using Microsoft.Owin.Security.OAuth;

namespace ProductionWebService.Helpers
{
    public class RequestHelper
    {
        public static int? GetUserId(HttpRequestContext context)
        {
            int? UserId = null;
            var Identity = context.Principal.Identity;
            return GetUserId(Identity);

            //var ClaimsIdentity = (System.Security.Claims.ClaimsIdentity)Identity;
            //var Claims = ClaimsIdentity.Claims;

            //var UserIdClaim = Claims.Where(x => x.Type == OAuthConstants.CLAIM_PROPERTY_USER_ID_KEY).FirstOrDefault();

            //if(UserIdClaim != null)
            //{
            //    UserId = Convert.ToInt32(UserIdClaim.Value);
            //}

            //return UserId;
        }

        public static int? GetUserId(OAuthTokenEndpointContext context)
        {
            int? UserId = null;
            var Identity = context.Identity;
            return GetUserId(Identity);
        }

        //private static int? GetUserId(System.Security.Principal.IIdentity Identity)
        private static int? GetUserId(IIdentity Identity)
        {
            int? UserId = null;

            var ClaimsIdentity = (System.Security.Claims.ClaimsIdentity)Identity;
            var Claims = ClaimsIdentity.Claims;

            var UserIdClaim = Claims.Where(x => x.Type == OAuthConstants.CLAIM_PROPERTY_USER_ID_KEY).FirstOrDefault();

            if (UserIdClaim != null)
            {
                UserId = Convert.ToInt32(UserIdClaim.Value);
            }

            return UserId;
        }

        public static int? GetUserId(HttpActionContext context)
        {
            return GetUserId(context.RequestContext);
        }

        public static string GetApplicationReturnType(HttpActionContext context)
        {
            string AcceptValue = null;
            AcceptValue = context.Request?.Headers?.Accept?.FirstOrDefault()?.MediaType;
            return AcceptValue;
        }
    }
}