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
    ///  An api controller to handle actions related to a course.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class CoursesController : ApiController
    {
        /// <summary>
        /// Deletes the course.
        /// </summary>
        /// <param name="id">The identifier of the course.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteCourse(int id)
        {
            using (var model = new graderEntities())
            {
                var courseExist = (from Course in model.Courses where Course.CourseID == id select Course).First();

                if (courseExist == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                foreach (CourseInstance ci in model.CourseInstances)
                {               
                    if (ci.CourseID == courseExist.CourseID)
                    {
                        foreach(StudentCourseRegistration scr in model.StudentCourseRegistrations.Where(s => s.CourseInstanceID == ci.CourseInstanceID))
                        {
                            foreach (var sub in model.SUBMISSIONs.Where(sb => sb.StudentCourseRegistrationID == scr.StudentCourseRegistrationID))
                            {
                                model.SUBMISSIONs.Remove(sub);
                            }
                            model.StudentCourseRegistrations.Remove(scr);
                        }
                        foreach (Course_Assignments ca in model.Course_Assignments.Where(c => c.CourseInstanceID == ci.CourseInstanceID))
                        {
                            model.Course_Assignments.Remove(ca);
                        }

                        model.CourseInstances.Remove(ci);
                    }
                }

                model.Courses.Remove(courseExist);
                model.SaveChanges();
            }
        }
    }
}