using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BobTheGrader.Models
{
    /// <summary>
    /// A model view class that contains various entities.
    /// </summary>
    public class MultiViewModel
    {
        /// <summary>
        /// Gets or sets the assignment.
        /// </summary>
        /// <value>
        /// The assignment.
        /// </value>
        public Assignment assignment { get; set; }
        /// <summary>
        /// Gets or sets the submission.
        /// </summary>
        /// <value>
        /// The submission.
        /// </value>
        public SUBMISSION submission { get; set; }
        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        /// <value>
        /// The question.
        /// </value>
        public Question question { get; set; }
        /// <summary>
        /// Gets or sets the course.
        /// </summary>
        /// <value>
        /// The course.
        /// </value>
        public Course course { get; set; }
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public User user { get; set; }
        /// <summary>
        /// Gets or sets a list of student course registrations.
        /// </summary>
        /// <value>
        /// The student course registration.
        /// </value>
        public IEnumerable<StudentCourseRegistration> SCRs { get; set; }
        /// <summary>
        /// Gets or sets the user roles.
        /// </summary>
        /// <value>
        /// The user roles.
        /// </value>
        public IEnumerable<UserRole> UserRoles { get; set; }
        /// <summary>
        /// Gets or sets the assignments.
        /// </summary>
        /// <value>
        /// The assignments.
        /// </value>
        public IEnumerable<Assignment> Assignments { get; set; }
        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public IEnumerable<User> Users { get; set; }
        /// <summary>
        /// Gets or sets the submissions.
        /// </summary>
        /// <value>
        /// The submissions.
        /// </value>
        public IEnumerable<SUBMISSION> Submissions { get; set; }
        /// <summary>
        /// Gets or sets the questions.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        public IEnumerable<Question> Questions { get; set; }
        /// <summary>
        /// Gets or sets the question types.
        /// </summary>
        /// <value>
        /// The question types.
        /// </value>
        public IEnumerable<QuestionType> QuestionTypes { get; set; }
        /// <summary>
        /// Gets or sets the course assignments.
        /// </summary>
        /// <value>
        /// The course assignments.
        /// </value>
        public IEnumerable<Course_Assignments> CourseAssignments { get; set; }
        /// <summary>
        /// Gets or sets the course assignment.
        /// </summary>
        /// <value>
        /// The course assignment.
        /// </value>
        public Course_Assignments CourseAssignment { get; set; }
        /// <summary>
        /// Gets or sets the course instances.
        /// </summary>
        /// <value>
        /// The course instances.
        /// </value>
        public IEnumerable<CourseInstance> CourseInstances { get; set; }
        /// <summary>
        /// Gets or sets the course instance.
        /// </summary>
        /// <value>
        /// The course instance.
        /// </value>
        public CourseInstance CourseInstance { get; set; }
        /// <summary>
        /// Gets or sets the courses.
        /// </summary>
        /// <value>
        /// The courses.
        /// </value>
        public IEnumerable<Course> Courses { get; set; }
        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        /// <value>
        /// The line.
        /// </value>
        public string Line { get; set; }
    }
}