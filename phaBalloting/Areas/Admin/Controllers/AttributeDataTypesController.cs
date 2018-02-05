using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using PagedList;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using phaBalloting.Data;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AttributeDataTypesController : Controller
    {
        
        private phaEntities db = new phaEntities();
        private UserActivity userActivity = new UserActivity();
        // GET: Admin/UnitDataTypes
        public ActionResult Index(int? pageNumber)
        {
            

            string datatypeName = string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower(); //Request.Form["DataTypeName"];
            
            var model = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();
            if (datatypeName != string.Empty)
            {
                return View(model.Where(x => x.Description.ToLower().Contains(datatypeName) || x.DataTypeName.ToLower().Contains(datatypeName) ).ToList().ToPagedList(pageNumber ?? 1, 10));
            }

            return View(model.ToPagedList(pageNumber ?? 1, 10));
        }

        // GET: Admin/UnitDataTypes/Details/5
        public ActionResult Details(int? id)
        {
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributeDataType unitDataType = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id==id).FirstOrDefault();
            if (unitDataType == null)
            {
                return HttpNotFound();
            }
            return View(unitDataType);
        }

        // GET: Admin/UnitDataTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/UnitDataTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DataTypeName,DataType,Description,UserId,EntryDate,ModifiedBy,ModifiedDate,IsActive,IsDeleted")] AttributeDataType unitDataType)
        {
            if (ModelState.IsValid)
            {
                unitDataType.UserId = User.Identity.GetUserId();
                unitDataType.EntryDate = DateTime.Now;
                unitDataType.IsActive = true;
                unitDataType.IsDeleted = false;
                db.AttributeDataTypes.Add(unitDataType);
                try {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Added Attribute Data Type with Id: " + unitDataType.Id + "Attribute Type: " + unitDataType.AttributesTypes);
                    return RedirectToAction("Index");

                }
                catch (System.Data.Entity.Validation.DbEntityValidationException invalids) {
                    foreach (System.Data.Entity.Validation.DbEntityValidationResult inavlidEntities in invalids.EntityValidationErrors)
                    {

                        foreach (var invalid in inavlidEntities.ValidationErrors)
                        {
                            ModelState.AddModelError(invalid.PropertyName, invalid.ErrorMessage);
                        }
                    }
                }
            }

            return View(unitDataType);
        }

        // GET: Admin/UnitDataTypes/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributeDataType unitDataType = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitDataType == null)
            {
                return HttpNotFound();
            }
            return View(unitDataType);
        }

        // POST: Admin/UnitDataTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DataTypeName,DataType,Description,UserId,EntryDate,ModifiedBy,ModifiedDate,IsActive,IsDeleted")] AttributeDataType unitDataType)
        {

            if (ModelState.IsValid)
            {
                unitDataType.ModifiedBy = User.Identity.GetUserId();
                unitDataType.ModifiedDate = DateTime.Now;
                db.Entry(unitDataType).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Edited Attribute Data Type with Id: " + unitDataType.Id + "Attribute Type: " + unitDataType.AttributesTypes);
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
            return View(unitDataType);
        }

        // GET: Admin/UnitDataTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributeDataType unitDataType = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitDataType == null)
            {
                return HttpNotFound();
            }
            return View(unitDataType);
        }

        // POST: Admin/UnitDataTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            AttributeDataType unitDataType = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitDataType == null)
            {
                return HttpNotFound();
            }

            unitDataType.IsDeleted = true;
            unitDataType.ModifiedBy = User.Identity.GetUserId();
            unitDataType.ModifiedDate = DateTime.Now;
            db.Entry(unitDataType).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Deleted Attribute Data Type with Id: " + unitDataType.Id + "Attribute Type: " + unitDataType.AttributesTypes);
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


        // GET: Admin/UnitDataTypes/Delete/5
        public ActionResult UndoDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttributeDataType unitDataType = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitDataType == null)
            {
                return HttpNotFound();
            }
            return View(unitDataType);
        }

        // POST: Admin/UnitDataTypes/Delete/5
        [HttpPost, ActionName("UndoDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult UndoDeleteConfirmed(int id)
        {

            AttributeDataType unitDataType = db.AttributeDataTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (unitDataType == null)
            {
                return HttpNotFound();
            }

            unitDataType.IsDeleted = false;
            unitDataType.ModifiedBy = User.Identity.GetUserId();
            unitDataType.ModifiedDate = DateTime.Now;
            db.Entry(unitDataType).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Undo Deleted Attribute Data Type with Id: " + unitDataType.Id + "Attribute Type: " + unitDataType.AttributesTypes);
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
