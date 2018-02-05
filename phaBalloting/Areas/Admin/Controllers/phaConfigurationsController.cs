using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using phaBalloting.Helpers;
using phaBalloting.Data;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class phaConfigurationsController : Controller
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/phaConfigurations
        public ActionResult Index()
        {
            return View(db.phaConfigurations.ToList());
        }

        // GET: Admin/phaConfigurations/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            phaConfiguration phaConfiguration = db.phaConfigurations.Find(id);
            if (phaConfiguration == null)
            {
                return HttpNotFound();
            }
            return View(phaConfiguration);
        }

        // GET: Admin/phaConfigurations/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/phaConfigurations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConfigKey,ConfigValue")] phaConfiguration phaConfiguration)
        {
            if (ModelState.IsValid)
            {
                db.phaConfigurations.Add(phaConfiguration);
                db.SaveChanges();
                UserHelper.WriteActivity("Added Configuration with Key: " + phaConfiguration.ConfigKey );
                return RedirectToAction("Index");
            }

            return View(phaConfiguration);
        }

        // GET: Admin/phaConfigurations/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            phaConfiguration phaConfiguration = db.phaConfigurations.Find(id);
            if (phaConfiguration == null)
            {
                return HttpNotFound();
            }
            return View(phaConfiguration);
        }

        // POST: Admin/phaConfigurations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConfigKey,ConfigValue")] phaConfiguration phaConfiguration)
        {
            if (ModelState.IsValid)
            {
                db.Entry(phaConfiguration).State = EntityState.Modified;
                db.SaveChanges();
                UserHelper.WriteActivity("Edited Configuration with Key: " + phaConfiguration.ConfigKey);
                return RedirectToAction("Index");
            }
            return View(phaConfiguration);
        }

        // GET: Admin/phaConfigurations/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            phaConfiguration phaConfiguration = db.phaConfigurations.Find(id);
            if (phaConfiguration == null)
            {
                return HttpNotFound();
            }
            return View(phaConfiguration);
        }

        // POST: Admin/phaConfigurations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            phaConfiguration phaConfiguration = db.phaConfigurations.Find(id);
            db.phaConfigurations.Remove(phaConfiguration);
            db.SaveChanges();
            UserHelper.WriteActivity("Deleted Configuration with Key: " + phaConfiguration.ConfigKey);
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
