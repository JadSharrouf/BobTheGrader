using BobTheGrader.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BobTheGrader.Controllers
{
    /// <summary>
    /// The teacher controller handles actions related to a teacher or an admin.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class TeachersController : Controller
    {
        string s = "";


        /// <summary>
        /// Teacher/Admin Index.
        /// </summary>
        /// <returns>Teacher home page view.</returns>
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

            if(user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            int userid = user.UserId;
            System.Web.HttpContext.Current.Session["TeacherID"] = System.Web.HttpContext.Current.User.Identity.Name;
            MultiViewModel MVM = new MultiViewModel();
            MVM.Assignments = GetAssignments();
            MVM.CourseAssignments = GetCourseAssignments();
            MVM.CourseInstances = GetCourseInstances().Where(ci => ci.TEACHERID == userid).OrderBy(c => c.CourseID).ThenBy(c => c.SectionNumber);
            MVM.Courses = GetCourses();
            return View(MVM);
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>The user that's signed in.</returns>
        public User getCurrentUser()
        {
            return new graderEntities().Users.Where(u => u.Email == System.Web.HttpContext.Current.User.Identity.Name).First();
        }


        /// <summary>
        /// Displays the help page.
        /// </summary>
        /// <returns>Help page view specific to the teacher.</returns>
        public ActionResult Help()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            return View();
        }

        /// <summary>
        /// Creating an assignment.
        /// </summary>
        /// <returns>A view that contains the form for creating or editing an assignment.</returns>
        public ActionResult New()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.CourseInstances = GetCourseInstances().Where(ci => ci.TEACHERID == user.UserId);
            MVM.Courses = GetCourses();
            MVM.CourseAssignments = GetCourseAssignments();
            MVM.assignment = new Assignment();
            MVM.assignment.DEADLINE = DateTime.Now;
            return View(MVM);
        }


        /// <summary>
        /// Saves the specified assignment.
        /// </summary>
        /// <param name="ass">MultiViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(MultiViewModel ass)
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
            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            Assignment assignment = null;

            using (var db = new graderEntities())
            {
                if (ass.assignment.AssignmentID == 0)
                {
                    assignment = ass.assignment;
                    db.Assignments.Add(assignment);
                }
                else
                {
                    assignment = (from Assignment in db.Assignments where Assignment.AssignmentID == ass.assignment.AssignmentID select Assignment).First();
                    assignment.DEADLINE = ass.assignment.DEADLINE;
                    assignment.Title = ass.assignment.Title;
                    assignment.Description = ass.assignment.Description;
                    assignment.Link = ass.assignment.Link;
                }
                string[] course_array = Request.Form.GetValues("course");
                if (course_array != null)
                {
                    foreach (string cid in course_array)
                    {
                        int id = int.Parse(cid);
                        var courseA = db.Course_Assignments.Where(ca => ca.AssignmentID == assignment.AssignmentID && ca.CourseInstanceID == id).FirstOrDefault();
                        if (courseA == null)
                        {
                            courseA = new Course_Assignments()
                            {
                                AssignmentID = assignment.AssignmentID,
                                CourseInstanceID = id
                            };
                            foreach (var question in db.Questions.Where(q => q.AssignmentID == courseA.AssignmentID))
                            {
                                foreach (var scr in db.StudentCourseRegistrations.Where(scr => scr.CourseInstanceID == id))
                                {
                                    //errors in submission fields (supposedly)
                                    SUBMISSION sub = new SUBMISSION();
                                    sub.StudentCourseRegistrationID = scr.StudentCourseRegistrationID;
                                    sub.GRADE = 0;
                                    sub.QuestionID = question.QuestionID;
                                    sub.TRIES = 0;
                                    sub.FILE = null;
                                    sub.SubmittedDate = null;
                                    sub.Result = null;
                                    db.SUBMISSIONs.Add(sub);
                                }
                            }
                            db.Course_Assignments.Add(courseA);
                        }
                    }
                }
                
                db.SaveChanges();
            }
            return RedirectToAction("ViewQuestions", "Teachers", new { id = ass.assignment.AssignmentID });
        }

        /// <summary>
        /// Edits the specific assignment.
        /// </summary>
        /// <param name="id">The  assignment identifier.</param>
        /// <returns>A view that contains the form for creating or editing an assignment.</returns>
        public ActionResult Edit(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            

            MultiViewModel MVM = new MultiViewModel();
            MVM.assignment = GetAssignmentById(id);
            MVM.CourseInstances = GetCourseInstances().Where(ci => ci.TEACHERID == user.UserId);
            MVM.CourseAssignments = GetCourseAssignments();
            MVM.Courses = GetCourses();

            if (MVM.assignment == null) { return HttpNotFound(); }

            return View("New", MVM);
        }

        /// <summary>
        /// Creates a new question.
        /// </summary>
        /// <param name="id">The identifier of the assignment that u want to add the question to.</param>
        /// <returns>A view that contains the form for creating or editing a question</returns>
        public ActionResult NewQuestion(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var types = GetTypes();
            MultiViewModel MVM = new MultiViewModel
            {
                QuestionTypes = types,
                question = new Question { AssignmentID = id }

            };

            return View(MVM);
        }


        /// <summary>
        /// Saves the Question.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult QuestionSave(Question question)
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
            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            using (var model = new graderEntities())
            {
                if (question.QuestionID == 0)
                {
                    
                    model.Questions.Add(question);
                    foreach (var cagm in model.Course_Assignments.Where(ca => ca.AssignmentID == question.AssignmentID))
                    {
                        foreach (var scr in model.StudentCourseRegistrations.Where(scr => scr.CourseInstanceID == cagm.CourseInstanceID))
                        {
                            //errors in submission fields (supposedly)
                            SUBMISSION sub = new SUBMISSION();
                            sub.StudentCourseRegistrationID = scr.StudentCourseRegistrationID;
                            sub.GRADE = 0;
                            sub.QuestionID = question.QuestionID;
                            sub.TRIES = 0;
                            sub.FILE = null;
                            sub.SubmittedDate = null;
                            sub.Result = null;
                            model.SUBMISSIONs.Add(sub);
                        }
                    }
                }

                else
                {
                    Question questionExist = (from Question in model.Questions where Question.QuestionID == question.QuestionID select Question).First();
                    questionExist.Title = question.Title;
                    questionExist.QuestionTypeID = question.QuestionTypeID;
                    questionExist.output = question.output;
                    int prevGrade = questionExist.MaximumGrade;
                    questionExist.MaximumGrade = question.MaximumGrade;
                    questionExist.MaximumTries = question.MaximumTries;
                    questionExist.AssignmentID = question.AssignmentID;
                    if(prevGrade != questionExist.MaximumGrade)
                    {
                        foreach (var sub in model.SUBMISSIONs.Where(s => s.QuestionID == questionExist.QuestionID))
                        {
                            sub.GRADE = (int) ((sub.GRADE * questionExist.MaximumGrade * 1.0) / prevGrade);
                        }
                    }
                }
                model.SaveChanges();
            }
            return RedirectToAction("ViewQuestions", "Teachers", new { id = question.AssignmentID });
        }

        /// <summary>
        /// Edits the question.
        /// </summary>
        /// <param name="id">The identifier of the question that is being edited.</param>
        /// <returns>A view that contains the form for creating or editing a question.</returns>
        public ActionResult QuestionEdit(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            Question question = GetQuestionById(id);
            string title = question.QuestionType.Title;
            //Debug.Write(title);
            IEnumerable<QuestionType> types = GetTypes();

            if (question == null) return HttpNotFound();

            MultiViewModel MVM = new MultiViewModel
            {
                QuestionTypes = types,
                question = question
            };

            return View("NewQuestion", MVM);
        }

        /// <summary>
        /// Views the questions.
        /// </summary>
        /// <param name="id">The identifier of the assignment whose questions are wanted.</param>
        /// <returns>A view that contains a table of all the questions of a certain assignment</returns>
        public ActionResult ViewQuestions(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.assignment = GetAssignmentById(id);
            MVM.Questions = GetQuestionsByAssignmentId(id);
            MVM.QuestionTypes = GetTypes();
            return View(MVM);
        }
        /// <summary>
        /// Displays the results of an assignment.
        /// </summary>
        /// <param name="id">The identifier of the course assignment whose results are wanted.</param>
        /// <returns>A view that contains the list of questions of the designated assignment</returns>
        public ActionResult AssignmentResults(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel mvm = new MultiViewModel();
            mvm.CourseAssignment = GetCourseAssignmentById(id);
            mvm.assignment = new graderEntities().Assignments.Where(a => a.AssignmentID == mvm.CourseAssignment.AssignmentID).First();
            mvm.CourseInstance = GetCourseInstances().Where(c => c.CourseInstanceID == mvm.CourseAssignment.CourseInstanceID && c.TEACHERID == user.UserId).First();
            mvm.Questions = GetQuestions().Where(q => q.AssignmentID == mvm.assignment.AssignmentID);
            return View(mvm);
        }

        /// <summary>
        /// Displays the results of the question
        /// </summary>
        /// <param name="ca">The identifier of the course assignment in which the question is in.</param>
        /// <param name="id">The identifier of the question whose results are wanted.</param>
        /// <returns>A view that contains the results and statistics of the designated question</returns>
        public ActionResult QuestionResults(int ca, int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.CourseAssignment = GetCourseAssignmentById(ca);
            MVM.assignment = new graderEntities().Assignments.Where(a => a.AssignmentID == MVM.CourseAssignment.AssignmentID).First();
            MVM.question = GetQuestionById(id); //GetQuestions().Where(q => q.AssignmentID == MVM.assignment.AssignmentID).First();
            MVM.Submissions = GetSubmissionsByQuestionID(id);
            MVM.Users = GetStudents();
            MVM.SCRs = GetSCR().Where(s => s.CourseInstanceID == MVM.CourseAssignment.CourseInstanceID);

            return View(MVM);
        }

        /// <summary>
        /// Displays the report of the designated assignment
        /// </summary>
        /// <param name="id">The identifier of the assignment whose results are wanted.</param>
        /// <returns>A view that contains the results and statistics of the designated assignment</returns>
        public ActionResult Report(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.CourseAssignment = GetCourseAssignmentById(id);
            MVM.assignment = new graderEntities().Assignments.Where(a => a.AssignmentID == MVM.CourseAssignment.AssignmentID).First();
            MVM.Questions = GetQuestions().Where(q => q.AssignmentID == MVM.assignment.AssignmentID);//GetQuestionsByAssignmentID(id);
            MVM.Submissions = GetSubmissions();
            //MVM.CourseAssignments = GetCourseAssignmentsByAssignmentId(id); not needed because 1 section
            MVM.CourseInstance = GetCourseInstances().Where(c => c.CourseInstanceID == MVM.CourseAssignment.CourseInstanceID && c.TEACHERID == user.UserId).First();
            MVM.SCRs = GetSCR().Where(s => s.CourseInstanceID == MVM.CourseInstance.CourseInstanceID);
            IEnumerable<User> users = GetStudents();
            List<User> users2 = new List<User>();
            foreach(StudentCourseRegistration scr in MVM.SCRs)
            {
                foreach(User student in users)
                {
                    if (student.UserId == scr.STUDENTID)
                    {
                        users2.Add(student);
                    }
                }             
            }
            MVM.Users = users2;
            return View(MVM);
        }

        /// <summary>
        /// Displays the assignments of the current user.
        /// </summary>
        /// <returns>A view that contains the assignments of the logged in teacher or admin</returns>
        public ActionResult Assignments()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            IEnumerable<Assignment> assignments = GetAssignments();
            return View(assignments);
        }

        /// <summary>
        /// Displays a list of the archived assignments.
        /// </summary>
        /// <returns>A view that contains the list of assignemnts created by a logged in teacher or admin whose deadlines have passed.</returns>
        public ActionResult Archive()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            IEnumerable<Assignment> assignments = GetAssignments();
            return View(assignments);
        }

        /// <summary>
        /// Dislays the courses.
        /// </summary>
        /// <returns>A view that contains the courses</returns>
        public ActionResult Courses()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.CourseInstances = GetCourseInstances();
            MVM.Courses = GetCourses();
            return View(MVM);
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <returns>A view that contains the form that is responsible for creating/editing a course</returns>
        public ActionResult NewCourse()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            Course course = new Course { Deadline = DateTime.Now };
            return View(course);
        }

        /// <summary>
        /// Saves the selected course.
        /// </summary>
        /// <param name="course">The course.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CourseSave(Course course)
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
            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            using (var model = new graderEntities())
            {
                if (course.CourseID == 0)
                {
                    model.Courses.Add(course);
                }

                else
                {
                    Course courseExist = (from Course in model.Courses where Course.CourseID == course.CourseID select Course).First();
                    courseExist.Title = course.Title;
                    courseExist.Deadline = course.Deadline;
                    courseExist.Semester = course.Semester;
                    courseExist.CourseID = course.CourseID;
                }
                model.SaveChanges();
            }
            return RedirectToAction("ViewSections", "Teachers", new { id = course.CourseID });
        }
        /// <summary>
        /// Edits the selected course.
        /// </summary>
        /// <param name="id">The identifier of the course that is being edited.</param>
        /// <returns>A view that contains the form which is responsible for creating/editing a course</returns>
        public ActionResult CourseEdit(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            Course course = GetCourseById(id);
            if (course == null) return HttpNotFound();

            return View("NewCourse",course);
        }

        /// <summary>
        /// Views the sections.
        /// </summary>
        /// <param name="id">The identifier of the course whose sections are being being viewed.</param>
        /// <returns>A view containing a list of sections of the designated course</returns>
        public ActionResult ViewSections(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.CourseInstances = GetCourseInstancesByCourseId(id);
            MVM.course = GetCourseById(id);
            MVM.SCRs = GetSCR();
            return View(MVM);
        }

        /// <summary>
        /// Creates a new section.
        /// </summary>
        /// <param name="id">The indentifier of the course that you want to create a section in.</param>
        /// <returns>A View that contains a form which is responsible for creating/editing a section</returns>
        public ActionResult NewSection(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.course = new Course() { CourseID = id };
            MVM.CourseInstance = new CourseInstance { CourseID = id };
            MVM.Users = new graderEntities().Users;
            ViewBag.message = s;
            return View(MVM);
        }

        /// <summary>
        /// Saves the section.
        /// </summary>
        /// <param name="f">The file.</param>
        /// <param name="mvm">The Multi View Model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveSection(HttpPostedFileBase f, MultiViewModel mvm)
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
            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            using (var model = new graderEntities())
            {
                if (mvm.CourseInstance.CourseInstanceID == 0)
                {
                    ViewBag.message = "<p style='color: #d9230f'>Section Created</p>";
                    model.CourseInstances.Add(mvm.CourseInstance);
                    model.SaveChanges();
                    mvm.CourseInstance.CourseInstanceID = model.CourseInstances.Where(ci => ci.CourseID == mvm.CourseInstance.CourseID && ci.TEACHERID == mvm.CourseInstance.TEACHERID).Select(u => u.CourseInstanceID).DefaultIfEmpty().Max();
                }
                else
                {
                    ViewBag.message = "<p style='color: #d9230f'>Changes Saved</p>";
                    CourseInstance exist = (from CourseInstance in model.CourseInstances where CourseInstance.CourseInstanceID == mvm.CourseInstance.CourseInstanceID select CourseInstance).First();
                    exist.CourseID = mvm.CourseInstance.CourseID;
                    exist.TEACHERID = mvm.CourseInstance.TEACHERID;
                    exist.SectionNumber = mvm.CourseInstance.SectionNumber;
                }

                if (Request.Files.Count != 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        string extension = Path.GetExtension(file.FileName);
                        if (extension == ".txt")
                        {
                            DirectoryInfo dir = new DirectoryInfo(@"~\Files\SCR\" + mvm.CourseInstance.CourseInstanceID);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            if (dir.Exists)
                            {
                                foreach (FileInfo fil in dir.GetFiles()) fil.Delete();
                            }
                            else
                            {
                                Directory.CreateDirectory(Server.MapPath(dir + ""));
                            }
                            string path = @"~\Files\SCR\" + mvm.CourseInstance.CourseInstanceID;
                            path = Path.Combine(Server.MapPath(path), file.FileName);
                            // var files = Directory.GetFiles(@"D:\home\site\wwwroot\App_Data");
                            List<string> emails = new List<string>();
                            file.SaveAs(path);
                            StreamReader reader = new StreamReader(path);
                            string name = reader.ReadLine();
                            if (name != null)
                            {
                                while (name != null)
                                {
                                    var student = model.Users.Where(u => u.UserRoleID == 2 && u.Email.ToLower().Equals(name.ToLower())).FirstOrDefault();
                                    if (student != null)
                                    {

                                        if (model.StudentCourseRegistrations.Where(scr => scr.STUDENTID == student.UserId && scr.CourseInstanceID == mvm.CourseInstance.CourseInstanceID).FirstOrDefault() == null)
                                        {
                                            StudentCourseRegistration scr = new StudentCourseRegistration
                                            {
                                                STUDENTID = student.UserId,
                                                CourseInstanceID = mvm.CourseInstance.CourseInstanceID
                                            };
                                            model.StudentCourseRegistrations.Add(scr);

                                            foreach (var cas in model.Course_Assignments.Where(ca => ca.CourseInstanceID == mvm.CourseInstance.CourseInstanceID))
                                            {
                                                foreach (var question in model.Questions.Where(q => q.AssignmentID == cas.AssignmentID))
                                                {
                                                    SUBMISSION sub = new SUBMISSION();
                                                    sub.StudentCourseRegistrationID = scr.StudentCourseRegistrationID;
                                                    sub.GRADE = 0;
                                                    sub.QuestionID = question.QuestionID;
                                                    sub.TRIES = 0;
                                                    sub.FILE = null;
                                                    sub.SubmittedDate = null;
                                                    sub.Result = null;
                                                    model.SUBMISSIONs.Add(sub);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        emails.Add(name);
                                    }
                                    model.SaveChanges();
                                    name = reader.ReadLine();
                                }
                            }
                            reader.Close();
                            if (emails.Count != 0)
                            {
                                ViewBag.message += "\n\nThe following emails do not exist in the databse and therefore cannot be linked to this course section:";
                                foreach (string mail in emails) s += "\n" + mail;
                            }
                            StreamWriter write = new StreamWriter(path);
                            foreach (var mail in emails)
                            {
                                write.WriteLine(mail);
                            }
                            write.Close();
                        }
                        else
                        {

                        }
                    }
                }
                model.SaveChanges();
            }
            return RedirectToAction("ViewSections","Teachers", new { id = mvm.CourseInstance.CourseID });
        }

        /// <summary>
        /// Edits the section.
        /// </summary>
        /// <param name="id">The identifier of the seciont that is being edited.</param>
        /// <returns>A view that contains a form which is reponsible for creating/editing a section</returns>
        public ActionResult CourseInstanceEdit(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.CourseInstance = new graderEntities().CourseInstances.Where(c => c.CourseInstanceID == id).FirstOrDefault();
            MVM.Users = new graderEntities().Users.Where(u => u.UserRoleID > 2);
            if (MVM.CourseInstance == null) return HttpNotFound();

            return View("NewSection", MVM);
        }

        /// <summary>
        /// Views the students.
        /// </summary>
        /// <param name="id">The identifier of the  course section whose students are being viewed.</param>
        /// <returns>A view that contains all the students of a specific course section</returns>
        public ActionResult ViewStudents(int id)
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            CourseInstance ci = GetCourseInstanceById(id);
            string course = new graderEntities().Courses.Where(c => c.CourseID == ci.CourseID).First().Title;
            string section = " - Section " + ci.SectionNumber;
            IEnumerable<StudentCourseRegistration> scrs = GetSCR().Where(s => s.CourseInstanceID == id);
            List<User> students = new List<User>();
            foreach(StudentCourseRegistration s in scrs)
            {
                User u = new graderEntities().Users.Where(a => a.UserId == s.STUDENTID).First();
                if (u.UserRoleID == 2) {
                    students.Add(u);
                }
            }
            ViewBag.message = course + section;
            MultiViewModel mvm = new MultiViewModel();
            mvm.Users = students;
            mvm.SCRs = scrs;
            return View(mvm);
        }

        /// <summary>
        /// Views the users of the entire application.
        /// </summary>
        /// <returns>A view that contains all the registered users of the website</returns>
        public ActionResult ViewUsers()
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

            if (user.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.Users = GetUsers();
            MVM.UserRoles = GetUserRoles();
            return View(MVM);
        }

        /// <summary>
        /// Saves the role of the user.
        /// </summary>
        /// <param name="mvm">The Multi View Model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RoleSave(MultiViewModel mvm)
        {
            User userM;
            try
            {
                userM = getCurrentUser();
            }
            catch (Exception e)
            {
                return RedirectToAction("Login", "Account");
            }
            if (userM.UserRoleID <= 2)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            try
            {
                using (var db = new graderEntities())
                {
                    User user = (from User in db.Users where User.UserId == mvm.user.UserId select User).First();
                    user.UserRoleID = mvm.user.UserRoleID;
                    if(mvm.user.UserRoleID == 2)
                    {
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
                                        if (mail.ToLower().Equals(user.Email.ToLower()) && db.StudentCourseRegistrations.Where(s => s.CourseInstanceID == ci.CourseInstanceID && s.STUDENTID == user.UserId).FirstOrDefault() == null)
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
                    }
                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }

            return RedirectToAction("ViewUsers", "Teachers");
        }

        /// <summary>
        /// Changes the role.
        /// </summary>
        /// <param name="id">The identifier of the user whose role is being changed.</param>
        /// <returns>A view that contains a form which allows the admin to change the role of the other users</returns>
        public ActionResult ChangeRole(int id)
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

            if (user.UserRoleID <= 3)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            MultiViewModel MVM = new MultiViewModel();
            MVM.user = GetUserByID(id);
            MVM.UserRoles = GetUserRoles();
            return View(MVM);
        }

        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="id">The identifier of the user.</param>
        /// <returns>The user with that ID</returns>
        private User GetUserByID(int id)
        {
            return new graderEntities().Users.Where(u => u.UserId == id).First();
        }

        /// <summary>
        /// Gets the user roles.
        /// </summary>
        /// <returns>A list of the roles that a user can adopt</returns>
        private IEnumerable<UserRole> GetUserRoles()
        {
            return new graderEntities().UserRoles;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <returns>A list of all users</returns>
        private IEnumerable<User> GetUsers()
        {
            return new graderEntities().Users;
        }

        /// <summary>
        /// Gets the course instances by course identifier.
        /// </summary>
        /// <param name="id">The identifier of the course.</param>
        /// <returns>A list of the sections that are in the selected course</returns>
        private IEnumerable<CourseInstance> GetCourseInstancesByCourseId(int id)
        {
            return new graderEntities().CourseInstances.Where(c => c.CourseID == id);
        }

        /// <summary>
        /// Gets the course by identifier.
        /// </summary>
        /// <param name="id">The identifier of the course.</param>
        /// <returns>The course</returns>
        public Course GetCourseById(int id)
        {
            return new graderEntities().Courses.Where(c => c.CourseID == id).First();
        }

        /// <summary>
        /// Gets the submissions by question identifier.
        /// </summary>
        /// <param name="id">The identifier of the Question.</param>
        /// <returns>A list of submissions of the designated question</returns>
        public IEnumerable<SUBMISSION> GetSubmissionsByQuestionID(int id)
        {
            return new graderEntities().SUBMISSIONs.Where(s => s.QuestionID == id);
        }
        /// <summary>
        /// Gets the assignments.
        /// </summary>
        /// <returns>A list of assignments</returns>
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
        /// Gets the course assignment by identifier.
        /// </summary>
        /// <param name="id">The identifier of a course assignment.</param>
        /// <returns>The course assignment</returns>
        public Course_Assignments GetCourseAssignmentById(int id)
        {
            return new graderEntities().Course_Assignments.Where(c => c.Course_AssignmentsID == id).First();
        }
        /// <summary>
        /// Gets the course assignments.
        /// </summary>
        /// <returns>A lsit of course assignments</returns>
        public IEnumerable<Course_Assignments> GetCourseAssignments()
        {
            return new graderEntities().Course_Assignments;
        }
        /// <summary>
        /// Gets the course assignments by assignment identifier.
        /// </summary>
        /// <param name="id">The identifier of the assignment.</param>
        /// <returns>The list of course assignments of the designated assignment</returns>
        public IEnumerable<Course_Assignments> GetCourseAssignmentsByAssignmentId(int id)
        {
            return new graderEntities().Course_Assignments.Where(p => p.AssignmentID == id);
        }
        public IEnumerable<CourseInstance> GetCourseInstances()
        {
            return new graderEntities().CourseInstances;
        }
        /// <summary>
        /// Gets the course instance by identifier.
        /// </summary>
        /// <param name="id">The identifier of the course instance.</param>
        /// <returns> The course Instance</returns>
        public CourseInstance GetCourseInstanceById(int id)
        {
            return new graderEntities().CourseInstances.Where(c => c.CourseInstanceID == id).First();
        }
        /// <summary>
        /// Gets the courses.
        /// </summary>
        /// <returns>A list of courses</returns>
        public IEnumerable<Course> GetCourses()
        {
            return new graderEntities().Courses;
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
        /// <returns>A list of questions of the designated assignemnt</returns>
        public IEnumerable<Question> GetQuestionsByAssignmentID(int id)
        {
            return new graderEntities().Questions.Where(q => q.AssignmentID == id);
        }
        /// <summary>
        /// Gets the questions by assignment identifier.
        /// </summary>
        /// <param name="id">The identifier of the assignment.</param>
        /// <returns>A list of all the questions of the designated assignment</returns>
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
        /// Gets the types of questions.
        /// </summary>
        /// <returns>A list of the types of questions</returns>
        public IEnumerable<QuestionType> GetTypes()
        {
            return new graderEntities().QuestionTypes;
        }
        /// <summary>
        /// Gets the students.
        /// </summary>
        /// <returns>Returns a list of students</returns>
        public IEnumerable<User> GetStudents()
        {
            return new graderEntities().Users.Where(u => u.UserRoleID == 2);
        }
        /// <summary>
        /// Gets the Student course registration.
        /// </summary>
        /// <returns>A list of student course registrations</returns>
        public IEnumerable<StudentCourseRegistration> GetSCR()
        {
            return new graderEntities().StudentCourseRegistrations;
        }
        /// <summary>
        /// Gets the submissions.
        /// </summary>
        /// <returns>A list of submissions</returns>
        public IEnumerable<SUBMISSION> GetSubmissions()
        {
            return new graderEntities().SUBMISSIONs;
        }
    }
}
