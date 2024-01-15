using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
//using Microsoft.Owin.Security.Google;
using ProductionWebService.Providers;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin;
using Owin;

using System.Net;

namespace ProductionWebService.Service.Auth
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            // TODO ?????
            //app.CreatePerOwinContext(ApplicationDbContext.Create);
            //app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            //app.UseCookieAuthentication(new CookieAuthenticationOptions());
            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            
            

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/api/token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                //AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60), //TimeSpan.FromDays(14),
                //MMC/ZA - 2017-04-28 - migrated into appconfig from compiler directives
                AllowInsecureHttp = Boolean.Parse(ConfigurationManager.AppSettings["AllowNonSSLAuth"]),
                

                // In production mode set AllowInsecureHttp = false
                //#if DEBUG
                //                AllowInsecureHttp = true
                //#else
                //                AllowInsecureHttp = false
                //#endif
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        }
    }
}