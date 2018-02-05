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
using Microsoft.AspNet.Identity;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AttributesTypesController : Controller //UnitTypes
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/UnitTypes
        public ActionResult Index(string option, string attributeName, int? pageNumber)
        {
            var unitTypes = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted).Include(u => u.AttributeDataType);
            var model=unitTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();
            attributeName= string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower();
            if (attributeName != string.Empty)
            {
                return View(model.Where(x => x.AttributeName.ToLower().Contains(attributeName) ||x.Description.ToLower().Contains(attributeName)).ToList().ToPagedList(pageNumber ?? 1, 10));
            }
            return View(model.ToPagedList(pageNumber ?? 1, 10));
        }

        // GET: Admin/UnitTypes/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributesType unitType = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitType == null)
            {
                return HttpNotFound();
            }
            return View(unitType);
        }

        // GET: Admin/UnitTypes/Create
        public ActionResult Create()
        {
            ViewBag.DataTypeId = new SelectList(db.AttributeDataTypes.Where(w=>!w.IsDeleted && w.IsActive), "Id", "DataTypeName");
            return View();
        }

        // POST: Admin/UnitTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AttributeName,Description,DataTypeId,UserId,EntryDate,ModifiedBy,ModifiedOn,IsActive,IsDeleted")] AttributesType unitType)
        {
            if (ModelState.IsValid)
            {
                unitType.UserId = User.Identity.GetUserId();
                unitType.EntryDate = DateTime.Now;
                unitType.IsActive = true;
                unitType.IsDeleted = false;
                db.AttributesTypes.Add(unitType);
                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Added Attribute Type with Id: " + unitType.Id + "Attribute Name: " + unitType.AttributeName);
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException invalids)
                {
                    foreach (System.Data.Entity.Validation.DbEntityValidationResult inavlidEntities in invalids.EntityValidationErrors)
                    {

                        foreach (var invalid in inavlidEntities.ValidationErrors)
                        {
                            ModelState.AddModelError(invalid.PropertyName, invalid.ErrorMessage);
                        }
                    }
                }
            }

            ViewBag.DataTypeId = new SelectList(db.AttributeDataTypes.Where(w=>w.IsActive && !w.IsDeleted), "Id", "DataTypeName", unitType.DataTypeId);
            return View(unitType);
        }

        // GET: Admin/UnitTypes/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributesType unitType = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();

            if (unitType == null)
            {
                return HttpNotFound();
            }
            ViewBag.DataTypeId = new SelectList(db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted), "Id", "DataTypeName", unitType.DataTypeId);
            return View(unitType);
        }

        // POST: Admin/UnitTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AttributeName,Description,DataTypeId,UserId,EntryDate,ModifiedBy,ModifiedOn,IsActive,IsDeleted")] AttributesType unitType)
        {
            if (ModelState.IsValid)
            {
                unitType.ModifiedBy = User.Identity.GetUserId();
                unitType.ModifiedOn = DateTime.Now;
                db.Entry(unitType).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Edited Attribute Type with Id: " + unitType.Id + "Attribute Type: " + unitType.AttributeName);
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException invalids)
                {
                    foreach (System.Data.Entity.Validation.DbEntityValidationResult inavlidEntities in invalids.EntityValidationErrors)
                    {

                        foreach (var invalid in inavlidEntities.ValidationErrors)
                        {
                            ModelState.AddModelError(invalid.PropertyName, invalid.ErrorMessage);
                        }
                    }
                }
            }
            ViewBag.DataTypeId = new SelectList(db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted), "Id", "DataTypeName", unitType.DataTypeId);
            return View(unitType);
        }

        // GET: Admin/UnitTypes/Delete/5
        public ActionResult Delete(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributesType unitType = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitType == null)
            {
                return HttpNotFound();
            }
            return View(unitType);
        }

        // POST: Admin/UnitTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            AttributesType unitType = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitType == null)
            {
                return HttpNotFound();
            }
            unitType.IsDeleted = true;
            unitType.ModifiedOn = DateTime.Now;
            unitType.ModifiedBy = User.Identity.GetUserId();

            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Deleted Attribute Type with Id: " + unitType.Id + "Attribute Type: " + unitType.AttributeName);
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException invalids)
            {
                foreach (System.Data.Entity.Validation.DbEntityValidationResult inavlidEntities in invalids.EntityValidationErrors)
                {

                    foreach (var invalid in inavlidEntities.ValidationErrors)
                    {
                        ModelState.AddModelError(invalid.PropertyName, invalid.ErrorMessage);
                    }
                }
            }
            return RedirectToAction("Delete", new { id = id });
        }

        // GET: Admin/UnitTypes/UndoDelete/5
        public ActionResult UndoDelete(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributesType unitType = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitType == null)
            {
                return HttpNotFound();
            }
            return View(unitType);
        }

        // POST: Admin/UnitTypes/UndoDelete/5
        [HttpPost, ActionName("UndoDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult UndoDeleteConfirmed(int id)
        {

            AttributesType unitType = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitType == null)
            {
                return HttpNotFound();
            }
            unitType.IsDeleted = false;
            unitType.ModifiedOn = DateTime.Now;
            unitType.ModifiedBy = User.Identity.GetUserId();

            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Undo Deleted Attribute Type with Id: " + unitType.Id + "Attribute Type: " + unitType.AttributeName);
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException invalids)
            {
                foreach (System.Data.Entity.Validation.DbEntityValidationResult inavlidEntities in invalids.EntityValidationErrors)
                {

                    foreach (var invalid in inavlidEntities.ValidationErrors)
                    {
                        ModelState.AddModelError(invalid.PropertyName, invalid.ErrorMessage);
                    }
                }
            }
            return RedirectToAction("UndoDelete", new { id = id });
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
