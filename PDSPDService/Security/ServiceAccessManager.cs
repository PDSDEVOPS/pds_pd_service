using System;
using System.Web;
using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using ProductionWebService.Persistence;
using ProductionWebService.Models;



namespace ProductionWebService.Security
{
    /// <summary>
    /// Checking user for external systems access to a given IP to a given route. (ExSysRouteAccess)
    /// </summary>
    public class ServiceAccessManager
    {
        /// <summary>
        /// Confirm external system access for the given user, route, and ip address
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="routeUrl"></param>
        /// <param name="IPAddress"></param>
        /// <returns></returns>
        public static bool HasAccess(int? userId, string routeUrl, string IPAddress)
        {
            return pullAccess(userId, routeUrl, IPAddress);
        }

        /// <summary>
        /// Confirm external system access for the given user, route, and ip address
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="routeUrl"></param>
        /// <param name="IPAddress"></param>
        /// <returns></returns>
        public static Task<bool> HasAccessAsync(int? userId, string routeUrl, string ipAddress)
        {
            return new Task<bool>(() => pullAccess(userId, routeUrl, ipAddress));
        }

        private static bool pullAccess(int? userId, string routeUrl, string ipAddress)
        {
            bool accessGranted = false;

            if( userId != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    accessGranted = (from a in db.ExSysRouteAccesses.AsNoTracking()
                                     where a.WebAPIUser.Id == userId &&
                                        (a.RouteUrl == null || routeUrl.StartsWith(a.RouteUrl)) &&
                                        (a.IPAddress == null || ipAddress.StartsWith(a.IPAddress)) &&
                                        (a.StopDt == null || a.StopDt.Value >= DateTime.Now)
                                     select a).Any();
                }
            }
            
            return accessGranted;
        }
    }

    public static class IPAddressHelper
    {
        public static string GetIPAddress()
        {
            string ip = String.Empty;
            try
            {
                if (OperationContext.Current != null)
                {
                    OperationContext context = OperationContext.Current;
                    MessageProperties prop = context.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    ip = endpoint.Address;
                }
                else if (HttpContext.Current != null)
                {
                    ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ip))
                    {
                        ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                    else
                    { // Using X-Forwarded-For last address
                        ip = ip.Split(',')
                               .Last()
                               .Trim();
                    }
                }
            }
            catch (Exception e)
            {
                //LATER - Probably Log this here at some point.
            }

            return ip;
        }
    }
}