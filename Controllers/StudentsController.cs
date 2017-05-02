using BobTheGrader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace BobTheGrader.Controllers
{
    /// <summary>
    /// The student controller handles actions related to a student.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class StudentsController : Controller
    {
        /// <summary>
        /// Student Index.
        /// </summary>
        /// <returns>Student home page view.</returns>
        public ActionResult Index()
        {
            User user;
            try
            {
                user = getCurrentUser();
            }
            catch (Exception e)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.UserRoleID != 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var entity = new graderEntities();
            MultiViewModel MVM = new MultiViewModel();
            MVM.SCRs = entity.StudentCourseRegistrations.Where(p => p.STUDENTID == user.UserId);
            MVM.CourseAssignments = GetCourseAssignments();
            MVM.CourseInstances = GetCourseInstances();
            MVM.Assignments = GetAssignments();
            MVM.Submissions = GetSubmissions();
            MVM.Courses = GetCourses();

            return View(MVM);
        }

        /// <summary>
        /// Displays the teachers and admins specific to the logged in student to for contact.
        /// </summary>
        /// <returns>Contact view page</returns>
        public ActionResult Contact()
        {
            User userC;
            try
            {
                userC = getCurrentUser();
            }
            catch (Exception e)
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (userC.UserRoleID != 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            ViewBag.Message = "Your contact page";
            User user = getCurrentUser();
            List<User> users = new List<User>();

            using(var model = new graderEntities())
            {
                foreach(User u in model.Users.Where(a => a.UserRoleID == 4))
                {
                    users.Add(u);
                }

                foreach(StudentCourseRegistration s in model.StudentCourseRegistrations.Where(a => a.STUDENTID == user.UserId))
                {
                    if(!users.Contains(model.Users.Where(u => u.UserId == model.CourseInstances.Where(c => c.CourseInstanceID == s.CourseInstanceID).FirstOrDefault().TEACHERID).FirstOrDefault())){
                        users.Add(model.Users.Where(u => u.UserId == model.CourseInstances.Where(c => c.CourseInstanceID == s.CourseInstanceID).FirstOrDefault().TEACHERID).FirstOrDefault());
                    }
                }
            }
            return View(users);
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>The user that's signed in.</returns>
        private User getCurrentUser()
        {
            return new graderEntities().Users.Where(u => u.Email == System.Web.HttpContext.Current.User.Identity.Name).First();
        }

        /// <summary>
        /// Displays the submission status.
        /// </summary>
        /// <param name="id">Assignment Identifier</param>
        /// <returns>The submission page view.</returns>
        public ActionResult Submit(int id)
        {
            User user;
            try
            {
                user = getCurrentUser();
            }
            catch (Exception e)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.UserRoleID != 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            Course_Assignments ca = GetCourseAssignments().Where(c => c.Course_AssignmentsID == id).First();
            StudentCourseRegistration str = GetSCR().Where(s => s.CourseInstanceID == ca.CourseInstanceID && s.STUDENTID == user.UserId).First();
            MVM.assignment = GetAssignmentById(ca.AssignmentID);
            MVM.Questions = GetQuestionsByAssignmentId(MVM.assignment.AssignmentID);
            MVM.Submissions = GetSubmissions().Where(s => s.StudentCourseRegistrationID == str.StudentCourseRegistrationID);

            return View(MVM);
        }

        /// <summary>
        /// Allows the student to submit his question file.
        /// </summary>
        /// <param name="file">The file that the student uploaded.</param>
        /// <param name="id">The identifier of the course assignment.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Submit(HttpPostedFileBase file, int id) {
            User user;
            try
            {
                user = getCurrentUser();
            }
            catch (Exception e)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.UserRoleID != 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            using (var model = new graderEntities())
            {
                Course_Assignments cas = (from Course_Assignments in model.Course_Assignments where Course_Assignments.Course_AssignmentsID == id select Course_Assignments).First();
                int aid = cas.AssignmentID;
                Assignment assignment = (from Assignment in model.Assignments where Assignment.AssignmentID == aid select Assignment).First();
                IEnumerable<Question> questions = (from Question in model.Questions where Question.AssignmentID == aid select Question).ToList();
                IEnumerable<SUBMISSION> subs = (from SUBMISSION in model.SUBMISSIONs select SUBMISSION).ToList();

                int qid = int.Parse(Request["questionID"]);
                int subid = int.Parse(Request["subID"]);
                SUBMISSION sub = (from SUBMISSION in model.SUBMISSIONs where SUBMISSION.SubmissionID == subid select SUBMISSION).First();
                if (file == null)
                {
                    return RedirectToAction("Submit");
                }

                if (file.ContentLength >= 0)
                {   
                    //-->should add a message "Already solved"
                    if (sub.Result != null)
                    {
                        if (sub.Result.Equals("Solved!"))
                        {
                            return RedirectToAction("Index");
                        }
                    }

                    Question question = (from Question in model.Questions where Question.QuestionID == qid select Question).First();

                    //-->should add a message "No more tries left"
                    if (question.MaximumTries == sub.TRIES)
                    {
                        return RedirectToAction("Index");
                    };

                    //-->should add a message "Deadline passed"
                    if (assignment.DEADLINE < DateTime.Now)
                    {
                        return RedirectToAction("Index");
                    }

                    sub.SubmittedDate = DateTime.Now;

                    string extension = Path.GetExtension(file.FileName);
                    // the reading happens through a text file, i think we can also read the static java file
                    //if needed
                    if (extension == ".zip")
                    {

                        var fileName = Path.GetFileName(file.FileName);
                        string path = Server.MapPath(@"~\Files") + "\\" + System.Web.HttpContext.Current.User.Identity.Name.Substring(0, System.Web.HttpContext.Current.User.Identity.Name.IndexOf("@")) + "\\" + assignment.AssignmentID + "\\" + question.QuestionID;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo di = new DirectoryInfo(path);
                            foreach (FileInfo fil in di.GetFiles()) fil.Delete();
                        }

                        Directory.CreateDirectory(Server.MapPath(@"~\Files") + "\\" + System.Web.HttpContext.Current.User.Identity.Name.Substring(0, System.Web.HttpContext.Current.User.Identity.Name.IndexOf("@")) + "\\" + assignment.AssignmentID + "\\" + question.QuestionID);
                        path = Path.Combine(path, fileName);
                        file.SaveAs(path);

                        ViewBag.HtmlStr = Request["submit"].Substring(17);

                        int line = Program.run(subid, path);
                        string output = "";
                        //right format
                        if (line > -10003)
                        {
                            sub.TRIES++;
                            if (line == 0)
                            {
                                output = "Solved!";
                                sub.GRADE = question.MaximumGrade - question.MaximumGrade * (sub.TRIES - 1) / question.MaximumTries;
                            }
                            else if (line == -10001) output = "No output provided.";
                            else if (line == -10002) output = "Compilation Error.";
                            else if (line > 0) output = "Wrong value(s) starting line " + line;
                            else output = "Excess of lines in output starting line " + (-line);
                        }
                        else if (line == -10006) output = "No Files Found.";
                        else if (line == -10005) output = "Directory Not Found. Please Resubmit.";
                        else if (line == -10004) output = "No Java File Found.";
                        else if (line == -10003) output = "Excess of Files. Non Java Files Found.";

                        sub.Result = output;
                        sub.FILE = Path.GetDirectoryName(path);
                        model.SaveChanges();

                        MVM.assignment = assignment;
                        MVM.Questions = questions;
                        MVM.Submissions = subs;
                        Thread.Sleep(3000);
                        return RedirectToAction("Submit","Students", new { id =  cas.Course_AssignmentsID});
                    }
                    else
                    {
                        sub.Result = "File submitted is not of correct format.";
                        model.SaveChanges();
                        return RedirectToAction("Submit", "Students", new { id = cas.Course_AssignmentsID });
                    }
                }
                else
                {
                    return RedirectToAction("Submit", "Students", new { id = cas.Course_AssignmentsID });
                }
            }
        }

        /// <summary>
        /// Gets the student-course registrations of a certain student.
        /// </summary>
        /// <param name="id">The identifier of the student.</param>
        /// <returns>The student-course registrations of a certain student</returns>
        public IEnumerable<StudentCourseRegistration> SCRbyStudentId(int id)
        {
            IEnumerable<StudentCourseRegistration> scr1;
            IEnumerable<StudentCourseRegistration> scr2 = null;
            using (var model = new graderEntities())
            {
                scr1 = model.StudentCourseRegistrations;
                foreach (StudentCourseRegistration s in scr1)
                {
                    if (s.STUDENTID == id)
                    {
                        scr2.ToList().Add(s);
                    }
                }
            }
            return scr2;
        }


        /// <summary>
        /// Gets the assignments.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Assignment> GetAssignments()
        {
            return new graderEntities().Assignments;
        }

        /// <summary>
        /// Gets the assignment by identifier.
        /// </summary>
        /// <param name="id">The identifier of the assignment.</param>
        /// <returns>The assignment</returns>
        public Assignment GetAssignmentById(int id)
        {
            return new graderEntities().Assignments.Single(a => a.AssignmentID == id);
        }
        /// <summary>
        /// Gets the course assignments.
        /// </summary>
        /// <returns>A list of all course assignments</returns>
        public IEnumerable<Course_Assignments> GetCourseAssignments()
        {
            return new graderEntities().Course_Assignments;
        }
        /// <summary>
        /// Gets the courses.
        /// </summary>
        /// <returns>A list of all courses</returns>
        public IEnumerable<Course> GetCourses()
        {
            return new graderEntities().Courses;
        }
        /// <summary>
        /// Gets the course assignments by assignment identifier.
        /// </summary>
        /// <param name="id">The identifier of the course assignment.</param>
        /// <returns>A list of course assignments</returns>
        public IEnumerable<Course_Assignments> GetCourseAssignmentsByAssignmentId(int id)
        {
            return new graderEntities().Course_Assignments.Where(p => p.AssignmentID == id);
        }
        /// <summary>
        /// Gets the course instances.
        /// </summary>
        /// <returns>A list of course instances</returns>
        public IEnumerable<CourseInstance> GetCourseInstances()
        {
            return new graderEntities().CourseInstances;
        }
        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <returns>A list of questions</returns>
        public IEnumerable<Question> GetQuestions()
        {
            return new graderEntities().Questions;
        }
        /// <summary>
        /// Gets the questions by assignment identifier.
        /// </summary>
        /// <param name="id">The identifier of the assignment.</param>
        /// <returns>A list of questions</returns>
        public IEnumerable<Question> GetQuestionsByAssignmentID(int id)
        {
            return new graderEntities().Questions.Where(q => q.AssignmentID == id);
        }
        /// <summary>
        /// Gets the questions by assignment identifier.
        /// </summary>
        /// <param name="id">The identifier of the assignment.</param>
        /// <returns>A list of questions</returns>
        public IEnumerable<Question> GetQuestionsByAssignmentId(int id)
        {
            return new graderEntities().Questions.Where(q => q.AssignmentID == id);
        }
        /// <summary>
        /// Gets the question by identifier.
        /// </summary>
        /// <param name="id">The identifier of the question.</param>
        /// <returns>The question</returns>
        public Question GetQuestionById(int id)
        {
            return new graderEntities().Questions.Single(q => q.QuestionID == id);
        }
        /// <summary>
        /// Gets the question types.
        /// </summary>
        /// <returns>A list of question types.</returns>
        public IEnumerable<QuestionType> GetTypes()
        {
            return new graderEntities().QuestionTypes;
        }
        /// <summary>
        /// Gets the students.
        /// </summary>
        /// <returns>A list of all students.</returns>
        public IEnumerable<User> GetStudents()
        {
            return new graderEntities().Users.Where(u => u.UserRoleID == 1);
        }
        /// <summary>
        /// Gets all the student course registrations.
        /// </summary>
        /// <returns>A list of student course registrations.</returns>
        public IEnumerable<StudentCourseRegistration> GetSCR()
        {
            return new graderEntities().StudentCourseRegistrations;
        }
        /// <summary>
        /// Gets all the submissions.
        /// </summary>
        /// <returns>A list of submissions.</returns>
        public IEnumerable<SUBMISSION> GetSubmissions()
        {
            return new graderEntities().SUBMISSIONs;
        }

    }
}