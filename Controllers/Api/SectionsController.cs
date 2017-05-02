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
    ///  An api controller to handle actions related to course sections.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class SectionsController : ApiController
    {
        /// <summary>
        /// Deletes the section.
        /// </summary>
        /// <param name="id">The identifier of the course section.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteSection(int id)
        {
            using (var model = new graderEntities())
            {
                var sectionExist = (from CourseInstance in model.CourseInstances where CourseInstance.CourseInstanceID == id select CourseInstance).First();

                if (sectionExist == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                foreach (var scr in model.StudentCourseRegistrations.Where(s => s.CourseInstanceID == sectionExist.CourseInstanceID))
                {
                    foreach(var sub in model.SUBMISSIONs.Where(sb => sb.StudentCourseRegistrationID == scr.StudentCourseRegistrationID))
                    {
                        model.SUBMISSIONs.Remove(sub);
                    }
                   
                    model.StudentCourseRegistrations.Remove(scr);
                }
                foreach (var c in model.Course_Assignments.Where(ca => ca.CourseInstanceID == sectionExist.CourseInstanceID))
                {
                    model.Course_Assignments.Remove(c);
                }

                model.CourseInstances.Remove(sectionExist);
                model.SaveChanges();
            }
        }
    }
}