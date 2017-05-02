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
    ///  An api controller to handle actions related to a question.
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class QuestionsController : ApiController
    {
        /// <summary>
        /// Deletes the question.
        /// </summary>
        /// <param name="id">The identifier of the question.</param>
        /// <exception cref="HttpResponseException"></exception>
        [HttpDelete]
        public void DeleteQuestion(int id)
        {
            using (var model = new graderEntities())
            {
                var questionExist = model.Questions.SingleOrDefault(q => q.QuestionID == id);

                if (questionExist == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                model.Questions.Remove(questionExist);
                model.SaveChanges();
            }
        }
    }
}
