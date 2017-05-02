using System;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace BobTheGrader.Models
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public string WebConfigRole { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (base.AuthorizeCore(httpContext))
            {
                /* Return true immediately if the authorization is not 
                locked down to any particular AD group */
                if (String.IsNullOrEmpty(WebConfigRole))
                    return base.AuthorizeCore(httpContext);

                foreach (string allowedGroup in WebConfigurationManager.AppSettings[WebConfigRole].Split(',').ToList<string>())
                {
                    if (allowedGroup.ToLower().StartsWith("user:"))
                    {
                        if (httpContext.User.Identity.Name.ToLower() == allowedGroup.Split(':')[1].ToLower())
                        {
                            return base.AuthorizeCore(httpContext);
                        }
                    }
                    else
                    {
                        //Here you can connect to your database and query the users in "allowedGroup" and check if the logged in user is in this group
                        if (true)
                            return base.AuthorizeCore(httpContext);
                    }
                }
            }
            return false; //returning false is equivaled to access denied
        }

    }
}