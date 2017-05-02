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
    /// An api controller to handle actions related to a student.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class studentSectionsController : ApiController
    {
        /// <summary>
        /// Deletes the student.
        /// </summary>
        /// <param name="id">The identifier of the student.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteStudent(int id)
        {
            using (var model = new graderEntities())
            {
                StudentCourseRegistration scr = model.StudentCourseRegistrations.Where(s => s.StudentCourseRegistrationID == id).First();

                if (scr == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                foreach (var sub in model.SUBMISSIONs.Where(sb => sb.StudentCourseRegistrationID == scr.StudentCourseRegistrationID))
                {
                    model.SUBMISSIONs.Remove(sub);
                }

                model.StudentCourseRegistrations.Remove(scr);
                model.SaveChanges();
            }
        }
    }
}