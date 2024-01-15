using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionWebService.Persistence;
using ProductionWebService.Models;

namespace ProductionWebService.Security
{
    public class UserAccessManager
    {
        public static bool HasAccess(int? userId)
        {
            bool accessGranted = false;

            if( userId != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    accessGranted = (from a in db.WebAPIUsers
                                     where a.Id == userId
                                     select a).Any();
                }
            }
            
            return accessGranted;
        }
    }
}