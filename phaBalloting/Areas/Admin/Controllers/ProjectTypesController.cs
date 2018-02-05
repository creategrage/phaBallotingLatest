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
using Microsoft.AspNet.Identity;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class ProjectTypesController : Controller //UnitTypes
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/UnitTypes
        public ActionResult Index(int? pageNumber)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }

            var ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted).Include(u => u.ProjectTypeConfigurations);
            var model = ProjectTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();
            var type= string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower();
            if (type != string.Empty)
            {
                return View(model.Where(x => x.Description.ToLower().Contains(type) || x.TypeName.ToLower().Contains(type)).ToList().ToPagedList(pageNumber ?? 1, 10));
            }
            //else
            //{
            //    return View(model.Where(x => x.UserName.StartsWith(searchString) || searchString == null).ToList().ToPagedList(pageNumber ?? 1, 25));
            //}

            return View(model.ToPagedList(pageNumber ?? 1, 10));
        }

        // GET: Admin/UnitTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectType ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (ProjectTypes == null)
            {
                return HttpNotFound();
            }
            return View(ProjectTypes);
        }

        // GET: Admin/UnitTypes/Create
        public ActionResult Create()
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            //ViewBag.DataTypeId = new SelectList(db.AttributeDataTypes.Where(w=>!w.IsDeleted && w.IsActive), "Id", "DataTypeName");
            ViewBag.AttributesTypes = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();

            return View();
        }

        // POST: Admin/UnitTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TypeName,Description,DataTypeId,UserId,EntryDate,ModifiedBy,ModifiedDate,IsActive,IsDeleted")] ProjectType ProjectTypes)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }

            var attributes = Request.Form["SelectedAttributes"];
            if (string.IsNullOrEmpty(attributes))
            {
                ModelState.AddModelError("", "Please select atleast one attribute to continue.");
            }
            if (ModelState.IsValid)
            {
                ProjectTypes.UserId = User.Identity.GetUserId();
                ProjectTypes.EntryDate = DateTime.Now;
                ProjectTypes.IsActive = true;
                ProjectTypes.IsDeleted = false;
                if (!String.IsNullOrEmpty(attributes))
                {
                    var splittedAttributes = attributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var attrib in splittedAttributes)
                    {
                        ProjectTypes.ProjectTypeConfigurations.Add(new ProjectTypeConfiguration { AttributeTypeId = int.Parse(attrib), Description = string.Empty, EntryDate = DateTime.Now, IsActive = true, IsDeleted = false, UserId = User.Identity.GetUserId() });
                    }
                }
                db.ProjectTypes.Add(ProjectTypes);

                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Added Project Type of Id:" + ProjectTypes.Id + " Project Name: " + ProjectTypes.TypeName);
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
            ViewBag.AttributesTypes = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();
            return View(ProjectTypes);
        }

        // GET: Admin/UnitTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectType ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();

            if (ProjectTypes == null)
            {
                return HttpNotFound();
            }
            ViewBag.AttributesTypes = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();
            return View(ProjectTypes);
        }

        // POST: Admin/UnitTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TypeName,Description,DataTypeId,UserId,EntryDate,ModifiedBy,ModifiedOn,IsActive,IsDeleted")] ProjectType ProjectTypes)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            var attributes = Request.Form["SelectedAttributes"];
            if (string.IsNullOrEmpty(attributes)) {
                ModelState.AddModelError("","Please select atleast one attribute to continue.");
            }
            if (ModelState.IsValid)
            {
                ProjectTypes.ModifiedBy = User.Identity.GetUserId();
                ProjectTypes.ModifiedDate = DateTime.Now;
               var existings= db.ProjectTypeConfigurations.Where(w=>w.PojectTypeId==ProjectTypes.Id);
                foreach (var ex in existings) {
                    db.ProjectTypeConfigurations.Remove(ex);
                }
                ProjectTypes.ProjectTypeConfigurations.Clear();
                if (!String.IsNullOrEmpty(attributes))
                {
                    var splittedAttributes = attributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var attrib in splittedAttributes)
                    {
                        db.ProjectTypeConfigurations.Add(new ProjectTypeConfiguration { PojectTypeId=ProjectTypes.Id, AttributeTypeId = int.Parse(attrib), Description = string.Empty, EntryDate = DateTime.Now, IsActive = true, IsDeleted = false, UserId = User.Identity.GetUserId() });
                    }
                }

                db.Entry(ProjectTypes).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Edited Project Type of Id:" + ProjectTypes.Id + " Project Name: " + ProjectTypes.TypeName);
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
            ViewBag.AttributesTypes = db.AttributesTypes.Where(w => w.IsActive && !w.IsDeleted).ToList();
            return View(ProjectTypes);
        }

        // GET: Admin/UnitTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectType ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (ProjectTypes == null)
            {
                return HttpNotFound();
            }
            return View(ProjectTypes);
        }

        // POST: Admin/UnitTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }
            ProjectType ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (ProjectTypes == null)
            {
                return HttpNotFound();
            }
            ProjectTypes.IsDeleted = true;
            ProjectTypes.ModifiedDate = DateTime.Now;
            ProjectTypes.ModifiedBy = User.Identity.GetUserId();

            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Deleted Project Type of Id:" + id + " Project Name: " + ProjectTypes.TypeName);
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
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectType ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (ProjectTypes == null)
            {
                return HttpNotFound();
            }
            return View(ProjectTypes);
        }

        // POST: Admin/UnitTypes/UndoDelete/5
        [HttpPost, ActionName("UndoDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult UndoDeleteConfirmed(int id)
        {
            if (!EnumManager.Modules.ProjectType.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }
            ProjectType ProjectTypes = db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted && w.Id == id).FirstOrDefault();
            if (ProjectTypes == null)
            {
                return HttpNotFound();
            }
            ProjectTypes.IsDeleted = false;
            ProjectTypes.ModifiedDate = DateTime.Now;
            ProjectTypes.ModifiedBy = User.Identity.GetUserId();

            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Undo Deletion of Id:"+id+" Project Name: "+ProjectTypes.TypeName);
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
