using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using phaBalloting.Data;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class UserActivitiesController : Controller
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/UserActivities
        public ActionResult Index(int? pageNumber)
        {
            
            var userActivities = db.UserActivities.ToList().OrderBy(x => x.Id);
            var name = string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower();

            //var location= string.IsNullOrEmpty(Request.Form["ProjectLocation"]) ? string.Empty : Request.Form["ProjectLocation"]; ;
            if (name != string.Empty)
            {
                return View(userActivities.Where(x => x.Description.ToLower().Contains(name) || x.ClientDetail.ToLower().Contains(name) ).ToList().ToPagedList(pageNumber ?? 1, 10));
            }

            return View(userActivities.ToPagedList(pageNumber ?? 1, 10));

        }

        // GET: Admin/UserActivities/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UserActivity userActivity = db.UserActivities.Find(id);
        //    if (userActivity == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userActivity);
        //}

        // GET: Admin/UserActivities/Details/userId
        public ActionResult Details(string username)
        {
            string url = Request.QueryString["username"];
            if (username == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserActivity userActivity = db.UserActivities.Where(a=>a.UserId==url).FirstOrDefault();
            if (userActivity == null)
            {
                return HttpNotFound();
            }
            return View(userActivity);
        }

        // GET: Admin/UserActivities/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/UserActivities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,EntryDate,Description,ClientDetail")] UserActivity userActivity)
        {
            if (ModelState.IsValid)
            {
                db.UserActivities.Add(userActivity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userActivity);
        }

        // GET: Admin/UserActivities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserActivity userActivity = db.UserActivities.Find(id);
            if (userActivity == null)
            {
                return HttpNotFound();
            }
            return View(userActivity);
        }

        // POST: Admin/UserActivities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,EntryDate,Description,ClientDetail")] UserActivity userActivity)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userActivity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userActivity);
        }

        // GET: Admin/UserActivities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserActivity userActivity = db.UserActivities.Find(id);
            if (userActivity == null)
            {
                return HttpNotFound();
            }
            return View(userActivity);
        }

        // POST: Admin/UserActivities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserActivity userActivity = db.UserActivities.Find(id);
            db.UserActivities.Remove(userActivity);
            db.SaveChanges();
            return RedirectToAction("Index");
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
