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
    ///  An api controller to handle actions related to a teacher.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class TeachersController : ApiController
    {
        /// <summary>
        /// Deletes the assignment.
        /// </summary>
        /// <param name="id">The identifier of the assignment.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteAssignment(int id)
        {
            using (var model = new graderEntities())
            {
                var assignmentExist = (from Assignment in model.Assignments where Assignment.AssignmentID == id select Assignment).First();

                if (assignmentExist == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                foreach (Question q in model.Questions.Where(q => q.AssignmentID == assignmentExist.AssignmentID))
                {
                    foreach (SUBMISSION sub in model.SUBMISSIONs.Where(s => s.QuestionID == q.QuestionID))
                    {
                        model.SUBMISSIONs.Remove(sub);
                    }
                    model.Questions.Remove(q);
                }

                model.Assignments.Remove(assignmentExist);
                model.SaveChanges();
            }
        }
    }
}
