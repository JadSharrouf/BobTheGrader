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
    ///  An api controller to handle actions related to a course assignment.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class CourseAssignmentsController : ApiController
    {
        /// <summary>
        /// Deletes the course assignment.
        /// </summary>
        /// <param name="id">The identifier of the course assignment.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteAssignment(int id)
        {
            using (var model = new graderEntities())
            {
                var assignmentExist = (from Course_Assignments in model.Course_Assignments where Course_Assignments.Course_AssignmentsID == id select Course_Assignments).First();

                if (assignmentExist == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                foreach (Question q in model.Questions.Where(q => q.AssignmentID == assignmentExist.AssignmentID))
                {
                    foreach(StudentCourseRegistration scr in model.StudentCourseRegistrations.Where(s => s.CourseInstanceID == assignmentExist.CourseInstanceID))
                    {
                        SUBMISSION sub = model.SUBMISSIONs.Where(s => s.StudentCourseRegistrationID == scr.StudentCourseRegistrationID && s.QuestionID == q.QuestionID).First();
                        model.SUBMISSIONs.Remove(sub);
                    }
                }

                model.Course_Assignments.Remove(assignmentExist);
                model.SaveChanges();
            }
        }
    }
}
