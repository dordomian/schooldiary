using Newtonsoft.Json;
using SchoolDiary.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolDiary.DAL
{
    public class SchoolContext : DbContext
    {
        private Logging.ILogger _logger = new Logging.Logger();
        private Logging.DBLogger _dbLogger = null;

        public SchoolContext() {
            _dbLogger = new Logging.DBLogger(this);
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Instructors).WithMany(i => i.Courses)
                .Map(t => t.MapLeftKey("CourseID")
                    .MapRightKey("InstructorID")
                    .ToTable("CourseInstructor"));

            modelBuilder.Entity<Faculty>().MapToStoredProcedures();
        }

        public override int SaveChanges()
        {
           try
            {
                _dbLogger.LoggingProcess().Wait();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error while trying save changes to Database logs");
            }
            return base.SaveChanges();
        }

        public int baseSaveChanges()
        {
            return base.SaveChanges();
        }
        
    }
}