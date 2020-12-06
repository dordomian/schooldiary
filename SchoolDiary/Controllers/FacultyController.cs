using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolDiary.DAL;
using SchoolDiary.Models;
using System.Data.Entity.Infrastructure;
using SchoolDiary.Repositories;

namespace SchoolDiary.Controllers
{
    public class FacultyController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Faculty
        public async Task<ActionResult> Index()
        {
            var faculties = db.Faculties.Include(d => d.Administrator);
            return View(await faculties.ToListAsync());
        }

        // GET: Faculty/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string query = "SELECT * FROM Faculty WHERE FacultyID = @p0";
            Faculty faculty = await db.Faculties.SqlQuery(query, id).SingleOrDefaultAsync();

            if (faculty == null)
            {
                return HttpNotFound();
            }
            return View(faculty);
        }

        // GET: Faculty/Create
        public ActionResult Create()
        {
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName");
            return View();
        }

        // POST: Faculty/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FacultyID,Name,Budget,StartDate,InstructorID")] Faculty faculty)
        {
            if (ModelState.IsValid)
            {
                db.Faculties.Add(faculty);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", faculty.InstructorID);
            return View(faculty);
        }

        // GET: Faculty/Edit/1
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faculty faculty = await db.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return HttpNotFound();
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", faculty.InstructorID);
            return View(faculty);
        }

        // POST: Faculty/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, byte[] rowVersion)
        {
            string[] fieldsToBind = new string[] { "Name", "Budget", "StartDate", "InstructorID", "RowVersion" };

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var facultyToUpdate = await db.Faculties.FindAsync(id);
            if (facultyToUpdate == null)
            {
                Faculty deletedFaculty = new Faculty();
                TryUpdateModel(deletedFaculty, fieldsToBind);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The faculty was deleted by another user.");
                ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", deletedFaculty.InstructorID);
                return View(deletedFaculty);
            }

            if (TryUpdateModel(facultyToUpdate, fieldsToBind))
            {
                try
                {
                    db.Entry(facultyToUpdate).OriginalValues["RowVersion"] = rowVersion;
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Faculty)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The faculty was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Faculty)databaseEntry.ToObject();

                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", "Current value: "
                                + databaseValues.Name);
                        if (databaseValues.Budget != clientValues.Budget)
                            ModelState.AddModelError("Budget", "Current value: "
                                + String.Format("{0:c}", databaseValues.Budget));
                        if (databaseValues.StartDate != clientValues.StartDate)
                            ModelState.AddModelError("StartDate", "Current value: "
                                + String.Format("{0:d}", databaseValues.StartDate));
                        if (databaseValues.InstructorID != clientValues.InstructorID)
                            ModelState.AddModelError("InstructorID", "Current value: "
                                + db.Instructors.Find(databaseValues.InstructorID).FullName);
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                        facultyToUpdate.RowVersion = databaseValues.RowVersion;
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", facultyToUpdate.InstructorID);
            return View(facultyToUpdate);
        }

        // GET: Faculty/Delete/5
        public async Task<ActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faculty faculty = await db.Faculties.FindAsync(id);
            if (faculty == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Index");
                }
                return HttpNotFound();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
            }

            return View(faculty);
        }

        // POST: Faculty/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Faculty faculty)
        {
            try
            {
                db.Entry(faculty).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = faculty.FacultyID });
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                return View(faculty);
            }
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
