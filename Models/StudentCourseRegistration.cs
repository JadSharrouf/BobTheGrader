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
    
    public partial class StudentCourseRegistration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StudentCourseRegistration()
        {
            this.SUBMISSIONs = new HashSet<SUBMISSION>();
        }
    
        public int StudentCourseRegistrationID { get; set; }
        public int STUDENTID { get; set; }
        public int CourseInstanceID { get; set; }
    
        public virtual CourseInstance CourseInstance { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SUBMISSION> SUBMISSIONs { get; set; }
        public virtual User User { get; set; }
    }
}
