using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BobTheGrader.Models;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Web.UI;

namespace BobTheGrader.Controllers
{

    /// <summary>
    /// The Account controller handles login and registration of all users.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class AccountController : Controller
    {
        #region Private Properties 
        private graderEntities databaseManager = new graderEntities();
        #endregion
        #region Default Constructor    
        /// <summary>  
        /// Initializes a new instance of the <see cref="AccountController" /> class.    
        /// </summary>  
        public AccountController()
        {
        }
        #endregion
        #region Login methods    
        /// <summary>  
        /// GET: /Account/Login    
        /// </summary>  
        /// <param name="returnUrl">Return URL parameter</param>  
        /// <returns>Return login view</returns>  
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            try
            {
                // Verification.    
                if (this.Request.IsAuthenticated)
                {
                    //// Info.    
                    //string email = System.Web.HttpContext.Current.User.Identity.Name;
                    //User current = new graderEntities().Users.Where(u => u.Email == email).First();
                    //if (current.UserRoleID == 2)
                    //{
                    //    //Response.Redirect("~/Students/");

                    //    return this.RedirectToAction("Index", "Students");
                    //}
                    //else if (current.UserRoleID == 3 || current.UserRoleID == 4)
                    //{
                    //    //Response.Redirect("~/Teachers/");
                    //    return this.RedirectToAction("Index", "Teachers");
                    //}

                   return this.RedirectToLocal(returnUrl);
                }
            }
            catch (Exception ex)
            {
                // Info    
                Console.Write(ex);
            }
            // Info.    
            return this.View();
        }
        /// <summary>  
        /// POST: /Account/Login    
        /// </summary>  
        /// <param name="model">Model parameter</param>  
        /// <param name="returnUrl">Return URL parameter</param>  
        /// <returns>Return login view</returns>  
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            string mail = model.Email;
            string password = model.Password;

            bool checkEmail = false;
            bool checkPass = false;

            foreach (User u in new graderEntities().Users)
            {
                if (mail.ToLower().Equals(u.Email.ToLower()))
                {
                    checkEmail = true;
                    if (password.Equals(u.Password)) checkPass = true;
                    break;
                }
            }

            if (checkEmail && checkPass)
            {
                try
                {
                    // Verification.    
                    if (ModelState.IsValid)
                    {
                        // Initialization.    
                        var loginInfo = this.databaseManager.LoginByUsernamePassword(model.Email, model.Password).ToList();
                        // Verification.    
                        if (loginInfo != null && loginInfo.Count() > 0)
                        {
                            // Initialization.    
                            var logindetails = loginInfo.First();
                            // Login In.    
                            this.SignInUser(logindetails.email, false);
                            // Info.

                            string s = "";
                            string email = model.Email;
                            User current = new graderEntities().Users.Where(u => u.Email == email).First();
                            if (current.UserRoleID == 2)
                            {
                                //Response.Redirect("~/Students/");
                                s = "http://bobthegrader.azurewebsites.net/Students/Index";
                            }
                            else if (current.UserRoleID == 3 || current.UserRoleID == 4)
                            {
                                //Response.Redirect("~/Teachers/");
                                s = "http://bobthegrader.azurewebsites.net/Teachers/Index";
                            }
                            else
                            {
                                s = "http://bobthegrader.azurewebsites.net/Home/AccessDenied";
                            }


                            return this.RedirectToLocal(s);
                        }
                        else
                        {
                            // Setting.    
                            ModelState.AddModelError(string.Empty, "Invalid username or password.");
                        }
                    }
                }

                catch (Exception ex)
                {
                    throw new Exception("bob is real");
                }
            }
            else
            {
                ViewBag.message = "<p style='color: #d9230f'>Incorrect email or password.</p>";
                return View(model);
            }
            // If we got this far, something failed, redisplay form    
            //return this.View(model);
            return this.RedirectToLocal(returnUrl);
        }
        #endregion
        #region Log Out method.    
        /// <summary>  
        /// POST: /Account/LogOff    
        /// </summary>  
        /// <returns>Return log off action</returns>  
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //System.Web.HttpContext.Current.Session.Timeout = 10;
            User user;
            try
            {
                user = new graderEntities().Users.Where(u => u.Email == System.Web.HttpContext.Current.User.Identity.Name).First();
            }
            catch (Exception e)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Setting.    
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                // Sign Out.    
                authenticationManager.SignOut();
            }
            catch (Exception ex)
            {
                // Info    
                return RedirectToAction("Index", "Account");
            }
            // Info.    
            return this.RedirectToAction("Index", "Home");
        }
        #endregion
        #region Helpers    
        #region Sign In method.    
        /// <summary>  
        /// Sign In User method.    
        /// </summary>  
        /// <param name="username">Username parameter.</param>  
        /// <param name="isPersistent">Is persistent parameter.</param>  
        private void SignInUser(string username, bool isPersistent)
        {
            // Initialization.    
            var claims = new List<Claim>();
            try
            {
                // Setting    
                claims.Add(new Claim(ClaimTypes.Name, username));
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                // Sign In.    
                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);
            }
            catch (Exception ex)
            {
                // Info    
                throw ex;
            }
        }
        #endregion
        #region Redirect to local method.    
        /// <summary>  
        /// Redirect to local method.    
        /// </summary>  
        /// <param name="returnUrl">Return URL parameter.</param>  
        /// <returns>Return redirection action</returns>  
        private ActionResult RedirectToLocal(string returnUrl)
        {
            try
            {
                // Verification.    
                if (Url.IsLocalUrl(returnUrl))
                {
                    // Info.    
                    return this.Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                // Info    
                throw ex;
            }
            // Info.    
            //Thread.Sleep(3000);
            return this.Redirect(returnUrl);
        }
        #endregion
        #endregion
        /// <summary>
        /// Registers the client as a user.
        /// </summary>
        /// <returns>Registration page view.</returns>
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Registers the specified user.
        /// </summary>
        /// <param name="user">The user registering.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            string email = user.Email;
            bool check = false;

            foreach (User u in new graderEntities().Users) {
                if (email.ToLower().Equals(u.Email.ToLower()))
                {
                    check = true;
                    break;
                }
            }

            if (check == false)
            {
                using (var db = new graderEntities())
                {
                    user.UserRoleID = 2;
                    int max = db.Users.Select(u => u.UserId).DefaultIfEmpty().Max() + 1;
                    user.UserId = max;
                    try { db.Users.Add(user); }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        Response.Write("<script>alert('Email already exists!')</script>");
                        return RedirectToAction("Register", "Account");
                    }

                    foreach (Course c in db.Courses.Where(c => c.Deadline >= DateTime.Now))
                    {
                        foreach (CourseInstance ci in db.CourseInstances.Where(i => i.CourseID == c.CourseID))
                        {
                            DirectoryInfo dir = new DirectoryInfo(@"D:\home\site\wwwroot\Files\SCR\" + ci.CourseInstanceID);
                            if (dir.Exists)
                            {
                                StreamReader read = new StreamReader(dir.GetFiles().First().FullName);
                                List<string> left = new List<string>();
                                var mail = read.ReadLine();
                                bool change = false;
                                while (mail != null)
                                {
                                    if (mail.ToLower().Equals(user.Email.ToLower()))
                                    {
                                        change = true;
                                        StudentCourseRegistration scr = new StudentCourseRegistration
                                        {
                                            CourseInstanceID = ci.CourseInstanceID,
                                            STUDENTID = user.UserId
                                        };
                                        db.StudentCourseRegistrations.Add(scr);
                                        foreach (var casgm in db.Course_Assignments.Where(ca => ca.CourseInstanceID == scr.CourseInstanceID))
                                        {
                                            foreach (var q in db.Questions.Where(q => q.AssignmentID == casgm.AssignmentID))
                                            {
                                                SUBMISSION sub = new SUBMISSION();
                                                sub.StudentCourseRegistrationID = scr.StudentCourseRegistrationID;
                                                sub.GRADE = 0;
                                                sub.QuestionID = q.QuestionID;
                                                sub.TRIES = 0;
                                                sub.FILE = null;
                                                sub.SubmittedDate = null;
                                                sub.Result = null;
                                                db.SUBMISSIONs.Add(sub);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        left.Add(mail);
                                    }
                                    mail = read.ReadLine();
                                }
                                read.Close();
                                if (change)
                                {
                                    StreamWriter write = new StreamWriter(dir.GetFiles().First().FullName);
                                    foreach (var name in left)
                                    {
                                        write.WriteLine(name);
                                    };
                                    write.Close();
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            else
            {
                ViewBag.message = "<p style='color: #d9230f'>Email already exists.</p>";
                return View(user);
            }         
            return RedirectToAction("LogIn","Account");
        }
    }
}