//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BobTheGrader.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SUBMISSION
    {
        public int SubmissionID { get; set; }
        public int QuestionID { get; set; }
        public int StudentCourseRegistrationID { get; set; }
        public int TRIES { get; set; }
        public string FILE { get; set; }
        public Nullable<int> GRADE { get; set; }
        public Nullable<System.DateTime> SubmittedDate { get; set; }
        public string Result { get; set; }
    
        public virtual Question Question { get; set; }
        public virtual StudentCourseRegistration StudentCourseRegistration { get; set; }
    }
}