using BobTheGrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BobTheGrader.Controllers
{
    /// <summary>
    /// The home controller contains method related to the landing page of the client.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class HomeController : Controller
    {

        /// <summary>
        /// Home Index
        /// </summary>
        /// <returns>The landing page view</returns>
        public ActionResult Index()
        {
            User user;
            try
            {
                user = new graderEntities().Users.Where(u => u.Email == System.Web.HttpContext.Current.User.Identity.Name).First();

                if (user.UserRoleID == 2)
                {
                    return Redirect("http://bobthegrader.azurewebsites.net/Students/Index");
                }
                if (user.UserRoleID >=3)
                {
                    return RedirectToAction("Index", "Teachers");
                }
            }
            catch (Exception e)
            {

            }

            return View();
        }

        /// <summary>
        /// A page used for client role authentication; it is displayed if the client has no access to the page.
        /// </summary>
        /// <returns>Access Denied page view</returns>
        public ActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// A page that displays contact information of the software developers.
        /// </summary>
        /// <returns>Contact page view</returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}