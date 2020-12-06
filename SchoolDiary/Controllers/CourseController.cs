using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolDiary.DAL;
using SchoolDiary.Models;
using System.Data.Entity.Infrastructure;
using SchoolDiary.Repositories;

namespace SchoolDiary.Controllers
{
    public class CourseController : Controller
    {
        private SchoolContext db = new SchoolContext();
        private GenericUnitOfWork uow = null;

        public CourseController()
        {
            uow = new GenericUnitOfWork();
        }

        // GET: Course
        public ActionResult Index(int? SelectedFaculty)
        {
            var faculties = db.Faculties.OrderBy(q => q.Name).ToList();
            ViewBag.SelectedFaculty = new SelectList(faculties, "FacultyID", "Name", SelectedFaculty);
            int facultyID = SelectedFaculty.GetValueOrDefault();

            IQueryable<Course> courses = db.Courses
                .Where(c => !SelectedFaculty.HasValue || c.FacultyID == facultyID)
                .OrderBy(d => d.CourseID)
                .Include(d => d.Faculty);
            var sql = courses.ToString();
            return View(courses.ToList());
        }

        // GET: Course/Details/5
        public ActionResult Details(int? id)
        {
            Course course = uow.Repository<Course>().GetDetail(p => p.CourseID == id);
            if (course == null)
                return HttpNotFound();
            return View(course);
        }


        public ActionResult Create()
        {
            PopulateFacultiesDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseID,Title,Credits,FacultyID")]Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    uow.Repository<Course>().Add(course);
                    uow.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            PopulateFacultiesDropDownList(course.FacultyID);
            return View(course);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            PopulateFacultiesDropDownList(course.FacultyID);
            return View(course);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var courseToUpdate = db.Courses.Find(id);
            if (TryUpdateModel(courseToUpdate, "",
               new string[] { "Title", "Credits", "FacultyID" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateFacultiesDropDownList(courseToUpdate.FacultyID);
            return View(courseToUpdate);
        }

        private void PopulateFacultiesDropDownList(object selectedFaculty = null)
        {
            var facultiesQuery = from d in db.Faculties
                                   orderby d.Name
                                   select d;
            ViewBag.FacultyID = new SelectList(facultiesQuery, "FacultyID", "Name", selectedFaculty);
        }


        // GET: Course/Delete/5
        public ActionResult Delete(int? id)
        {
            Course course = uow.Repository<Course>().GetDetail(p => p.CourseID == id);
            if (course == null)
                return HttpNotFound();
            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Course course = uow.Repository<Course>().GetDetail(p => p.CourseID == id);
                uow.Repository<Course>().Delete(course);
                uow.SaveChanges();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        public ActionResult UpdateCourseCredits()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdateCourseCredits(int? multiplier)
        {
            if (multiplier != null)
            {
                ViewBag.RowsAffected = db.Database.ExecuteSqlCommand("UPDATE Course SET Credits = Credits * {0}", multiplier);
            }
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
