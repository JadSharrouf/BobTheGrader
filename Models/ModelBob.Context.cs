﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class graderEntities : DbContext
    {
        public graderEntities()
            : base("name=graderEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Assignment> Assignments { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Course_Assignments> Course_Assignments { get; set; }
        public virtual DbSet<CourseInstance> CourseInstances { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<QuestionType> QuestionTypes { get; set; }
        public virtual DbSet<StudentCourseRegistration> StudentCourseRegistrations { get; set; }
        public virtual DbSet<SUBMISSION> SUBMISSIONs { get; set; }
        public virtual DbSet<database_firewall_rules> database_firewall_rules { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<vwGrader> vwGraders { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Role_Permissions> Role_Permissions { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
    
        public virtual ObjectResult<LoginByUsernamePassword_Result> LoginByUsernamePassword(string email, string password)
        {
            var emailParameter = email != null ?
                new ObjectParameter("email", email) :
                new ObjectParameter("email", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<LoginByUsernamePassword_Result>("LoginByUsernamePassword", emailParameter, passwordParameter);
        }
    }
}
