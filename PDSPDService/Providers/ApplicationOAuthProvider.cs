using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using ProductionWebService.Persistence;
using ProductionWebService.Models;
using ProductionWebService.Helpers;
using System.IO;
using System.Web;
using log4net;

namespace ProductionWebService.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private ILog log = log4net.LogManager.GetLogger(typeof(ApplicationOAuthProvider));
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            // Initialization.
            string usernameVal = context.UserName;
            string passwordVal = context.Password;
            var user = LoginByUsernamePassword(usernameVal, passwordVal);

            // Verification.
            if (user == null )
            {   
                context.SetError("invalid_grant", "The user name or password is incorrect.");

                // Retuen info.
                return;
            }

            string ClientId = HttpContext.Current.Request[OAuthConstants.CLIENT_ID];

            // Initialization.
            var claims = new List<Claim>();

            // Setting
            claims.Add(new Claim(OAuthConstants.CLAIM_PROPERTY_USER_NAME_KEY, user.UserName));
            claims.Add(new Claim(OAuthConstants.CLAIM_PROPERTY_USER_ID_KEY, user.Id.ToString()));

            // Setting Claim Identities for OAUTH 2 protocol.
            ClaimsIdentity oAuthClaimIdentity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesClaimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType);

            // Setting user authentication.
            AuthenticationProperties properties = CreateProperties(user.UserName, user.Id, ClientId);
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthClaimIdentity, properties);

            // Grant access to authorize user.
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesClaimIdentity);

            //var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            #region For Reference
            try
            {

                //ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

                //if (user == null || (user.LockoutEnabled && DateTime.UtcNow < user.LockoutEndDateUtc))
                //{
                //    if (user == null)
                //    {
                //        // If we didn't find the user by username and password, See if we can find the user by username.
                //        // If we can, then update their lock count
                //        user = await userManager.FindByNameAsync(context.UserName);

                //        if (user != null)
                //        {
                //            const int MAX_FAILED_COUNT = 3;
                //            user.AccessFailedCount++;

                //            if (user.AccessFailedCount >= MAX_FAILED_COUNT)
                //            {
                //                user.LockoutEnabled = true;
                //                user.LockoutEndDateUtc = DateTime.MaxValue;
                //            }

                //            userManager.Update(user);
                //        }
                //    }
                //    else
                //    {
                //        // User was NOT null which means that he was already locked out. We might as well update his failed login count
                //        user.AccessFailedCount++;
                //        userManager.Update(user);
                //    }

                //    context.SetError("invalid_grant", "The user name or password is incorrect.");
                //    return;
                //}

                //string[] roleNames;
                //var roleIds = user.Roles.Select(x => x.RoleId).ToList();
                //// if a user doesn't have companies, they should, but i'm putting a -1 here so this code does not blow up
                //var senderCompany = user.Companies.FirstOrDefault();
                //var senderCompanyId = senderCompany != null ? senderCompany.Id : -1;
                //var UserCompanyName = senderCompany != null ? senderCompany.Name : String.Empty;
                //int receiverCompanyId = senderCompanyId;
                //int[] partnerCompanyIds = senderCompany != null ? senderCompany.Partners.Select(x => x.Id).ToArray() : new int[0];
                //// Bhu added
                //int defaultDashboard = user.UserPreferences.FirstOrDefault().DashboardTypeId;
                //var waterView = "Water";

                //using (var db = new ApplicationDbContext())
                //{
                //    roleNames = db.Roles.Where(x => roleIds.Contains(x.Id)).Select(x => x.Name).ToArray();
                //    var receiverCompany = db.Companies.FirstOrDefault(x => x.Partners.Select(y => y.Id).Contains(senderCompanyId));
                //    if (receiverCompany != null)
                //    {
                //        receiverCompanyId = receiverCompany.Id;
                //    }
                //    var roleId = user.Roles.FirstOrDefault().RoleId;
                //    var test = db.CompanyRoleViews.ToList();

                //    var crv = (from x in db.CompanyRoleViews
                //               where (x.RoleId == roleId) && (x.CompanyId == senderCompanyId)
                //               select x).FirstOrDefault();
                //    if (crv == null)
                //    {
                //        crv = (from x in db.CompanyRoleViews
                //               where (x.CompanyId == null) && (x.RoleId == roleId)
                //               select x).FirstOrDefault();
                //    }

                //    waterView = crv != null ? crv.WaterView : waterView;
                //}


                //user.LastLogin = DateTime.UtcNow;
                //user.AccessFailedCount = 0;

                //userManager.Update(user);

                ////string ClientId = HttpContext.Current.Request[OAuthConstants.CLIENT_ID];

                ////ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
                ////    OAuthDefaults.AuthenticationType);

                ////ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                ////    CookieAuthenticationDefaults.AuthenticationType);


                //oAuthIdentity.AddClaim(new Claim("senderId", senderCompanyId.ToString()));
                //oAuthIdentity.AddClaim(new Claim("receiverId", receiverCompanyId.ToString()));


                //AuthenticationProperties properties = CreateProperties(user.UserName, roleNames, user.Id, senderCompanyId,
                //    receiverCompanyId, user.FirstName ?? "", user.LastName ?? "", partnerCompanyIds, ClientId, user.Email, user.LastLogin, user.LastDownloadDate, user.PhoneNumber, user.DriverId, UserCompanyName, defaultDashboard, waterView, user.MobileAppLogModeId);

                ////AuthenticationTicket ticket = new AuthenticationTicket( oAuthIdentity, properties);

                ////context.Validated(ticket);
                ////context.Request.Context.Authentication.SignIn(cookiesIdentity);
            }
            catch (Exception ex)
            {
                String s = ex.Message;
            }
            #endregion For Reference
        }

        //public static void AddClaimsToClaimIdentity(ClaimsIdentity Identity, string userName, string userId)
        //{
        //    Identity.AddClaim(new Claim(OAuthConstants.CLAIM_PROPERTY_USER_NAME_KEY, userName));
        //    Identity.AddClaim(new Claim(OAuthConstants.CLAIM_PROPERTY_USER_ID_KEY, userId));
        //}

        /// <summary>
        /// Token endpoint override method
        /// </summary>
        /// <param name="context">Context parameter</param>
        /// <returns>Returns when task is completed</returns>
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            //var Identity = context.Identity;

            //var ClaimsIdentity = (System.Security.Claims.ClaimsIdentity)Identity;
            //var Claims = ClaimsIdentity.Claims;

            //var UserIdClaim = Claims.Where(x => x.Type == OAuthConstants.CLAIM_PROPERTY_USER_ID_KEY).FirstOrDefault();

            //if (UserIdClaim != null)
            //{
            //    var UserId = Convert.ToInt32(UserIdClaim.Value);
            //}

            

            //SetExpirationTime(context);

            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                // Adding.
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            // Return info.
            return Task.FromResult<object>(null);
        }

        public void SetExpirationTime(OAuthTokenEndpointContext context)
        {
            
            try
            {
                if (context.Properties.Dictionary.ContainsKey(OAuthConstants.CLIENT_ID) && !String.IsNullOrEmpty(context.Properties.Dictionary[OAuthConstants.CLIENT_ID]) && context.Properties.Dictionary[OAuthConstants.CLIENT_ID] != OAuthConstants.CLIENT_NOT_SPECIFIED)
                {
                    var UserId = RequestHelper.GetUserId(context);
                    String ClientId = context.Properties.Dictionary[OAuthConstants.CLIENT_ID];

                    using (var db = new ApplicationDbContext())
                    {
                        WebAPIClient APIClient = db.WebAPIClients.Where(wc => wc.ClientId == ClientId && wc.UserId == UserId).FirstOrDefault();

                        if (APIClient != null)
                        {
                            DateTime CurrentDateTime = DateTime.Now;
                            context.Properties.IssuedUtc = CurrentDateTime;
                            context.Properties.ExpiresUtc = CurrentDateTime.AddMinutes(APIClient.TokenExpirationMinutes);    // set the appropriate expiration date.
                        }
                        else
                        {
                            SetFallBackMinutesToDefault(context);
                        }
                    }
                }
                else
                {
                    SetFallBackMinutesToDefault(context);
                }
            }
            catch (Exception ex)
            {
                SetFallBackMinutesToDefault(context);

                String ClientId = String.Empty;

                // Log the error
                log.Error("ApplicationOAuthProvider : Exception occurred while attempting to set expiration date time.", ex);
            }
        }

        private void SetFallBackMinutesToDefault(OAuthTokenEndpointContext context)
        {
            // If there were issues then fall back to shorter expiration time
            const int FallBackMinutes = 10;
            DateTime CurrentDateTime = DateTime.Now;

            context.Properties.IssuedUtc = CurrentDateTime;
            context.Properties.ExpiresUtc = CurrentDateTime.AddMinutes(FallBackMinutes);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                // Validate Authoorization.
                context.Validated();
            }

            // Return info.
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Validate client redirect URI override method
        /// </summary>
        /// <param name="context">Context parmeter</param>
        /// <returns>Returns validation of client redirect URI</returns>
        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            // Verification.
            if (context.ClientId == _publicClientId)
            {
                // Initialization.
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                // Verification.
                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    // Validating.
                    context.Validated();
                }
            }

            // Return info.
            return Task.FromResult<object>(null);
        }

        private WebAPIUser LoginByUsernamePassword(string UserName, string Password)
        {
            using( var db = new ApplicationDbContext())
            {
                var User = db.WebAPIUsers.Where(x => x.UserName == UserName && x.Password == Password).FirstOrDefault();
                return User;
            }
        }

        /// <summary>
        /// Create Authentication properties method.
        /// </summary>
        /// <param name="userName">User name parameter</param>
        /// <returns>Returns authenticated properties.</returns>
        public static AuthenticationProperties CreateProperties(string userName, int UserId, string ClientId)
        {
            // Settings.
            IDictionary<string, string> data = new Dictionary<string, string>
                                               {
                                                   { "userName", userName },
                                                    {"UserId", UserId.ToString() },
                                                    {OAuthConstants.CLIENT_ID, ClientId }
                                               };

            // Return info.
            return new AuthenticationProperties(data);
        }
    }

    //public class User 
    //{
        

    //    public string UserName { get; set; }
    //}

}