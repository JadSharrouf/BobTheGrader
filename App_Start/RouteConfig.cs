using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BobTheGrader
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );


            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new
            //    {
            //        controller = "Account",
            //        action = "Login",
            //        id = UrlParameter.Optional
            //    }
            //);

            routes.MapRoute(
                name: "questionReport",
                url: "{controller}/{action}/{ca}/{id}",
                defaults: new { controller = "Teacher", action = "QuestionResults", id = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "deleteAssignment",
            //    url: "api/{controller}/{action}/{id}",
            //    defaults: new { controller = "Teacher", action = "DeleteAssignment", id = UrlParameter.Optional }
            //);
        }
    }
}
