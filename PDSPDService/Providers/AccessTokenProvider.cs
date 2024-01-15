using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.Infrastructure;
using ProductionWebService.Persistence;
using ProductionWebService.Models;
using log4net;

namespace ProductionWebService.Providers
{
    public class AccessTokenProvider : AuthenticationTokenProvider
    {
        private ILog log = log4net.LogManager.GetLogger(typeof(AccessTokenProvider));
        public override void Create(AuthenticationTokenCreateContext context)
        {
            try
            {
                String ClientId = context.Ticket.Properties.Dictionary[OAuthConstants.CLIENT_ID];

                if (!String.IsNullOrEmpty(ClientId) && ClientId != OAuthConstants.CLIENT_NOT_SPECIFIED)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        WebAPIClient APIClient = db.WebAPIClients.Where(wc => wc.ClientId == ClientId).FirstOrDefault();

                        if (APIClient != null)
                        {
                            DateTime CurrentDateTime = DateTime.Now;
                            context.Ticket.Properties.IssuedUtc = CurrentDateTime;
                            context.Ticket.Properties.ExpiresUtc = CurrentDateTime.AddMinutes(APIClient.TokenExpirationMinutes);    // set the appropriate expiration date.
                        }
                    }
                }
            }catch(Exception ex)
            {
                // If there were issues then fall back to longer expiration time so that the mobile and web aren't affected.
                const int FallBackMinutes = 720;
                context.Ticket.Properties.ExpiresUtc = DateTime.Now.AddMinutes(FallBackMinutes);
                context.SetToken(context.SerializeTicket());

                String ClientId = String.Empty;

                // Log the error
                log.Error("AccessTokenProvider : Exception occurred while attempting to create token.", ex);
            }
        }
    }
}