using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using phaBalloting.Data;
using PagedList;
using Microsoft.AspNet.Identity;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/Events
        public ActionResult Index(int? pageNumber)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }

            var model=db.Events.ToList();
            var name= string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower();
            var eventLocation = string.IsNullOrEmpty(Request.Form["EventLocation"]) ? string.Empty : Request.Form["EventLocation"];
            if (name != string.Empty)
            {
                return View(model.Where(x => x.EventName.ToLower().Contains(name) || x.EventLocation.ToLower().Contains(name)).ToList().ToPagedList(pageNumber ?? 1, 10));
            }
            else
            {
                return View(model.ToPagedList(pageNumber ?? 1, 10));
            }
        }

        // GET: Admin/Events/Details/5
        public ActionResult Details(int? id)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: Admin/Events/Create
        public ActionResult Create()
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            return View();
        }

        // POST: Admin/Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,EventName,EventLocation,HeldDate,UserId,EntryDate,LastModifiedOn,LastModifiedBy")] Event @event)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }

            @event.EntryDate = DateTime.Now;
            
            @event.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Events.Add(@event);
                db.SaveChanges();
                UserHelper.WriteActivity("Event Added of Id: " + @event.Id + " Name: " + @event.EventName);
                return RedirectToAction("Index");
            }

            return View(@event);
        }

        // GET: Admin/Events/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Admin/Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Event @event)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            @event.LastModifiedOn = DateTime.Now;
            @event.LastModifiedBy = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                UserHelper.WriteActivity("Event Modified of Id: " + @event.Id + " Name: " + @event.EventName);
                return RedirectToAction("Index");
            }
            return View(@event);
        }

        // GET: Admin/Events/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EnumManager.Modules.Events.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            Event @event = db.Events.Find(id);
            db.Events.Remove(@event);
            db.SaveChanges();
            UserHelper.WriteActivity("Event Deleted of Id: " + id + " Name: " + @event.EventName);
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
