using BobTheGrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BobTheGrader.Controllers.Api
{
    /// <summary>
    ///  An api controller to handle actions related to a user.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class UsersController : ApiController
    {
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="id">The identifier of the user.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteUser(int id)
        {
            using (var model = new graderEntities())
            {
                var user = (from User in model.Users where User.UserId == id select User).First();

                if (user == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                if (user.UserRoleID != 4 || model.Users.Where(u => u.UserRoleID == 4).Count() != 1)
                {
                    foreach (StudentCourseRegistration scr in model.StudentCourseRegistrations.Where(s => s.STUDENTID == id))
                    {
                        foreach (SUBMISSION s in model.SUBMISSIONs.Where(a => a.StudentCourseRegistrationID == scr.StudentCourseRegistrationID))
                        {
                            model.SUBMISSIONs.Remove(s);
                        }
                        model.StudentCourseRegistrations.Remove(scr);
                    }
                    model.Users.Remove(user);
                    model.SaveChanges();
                }   
            }
        }
    }
}