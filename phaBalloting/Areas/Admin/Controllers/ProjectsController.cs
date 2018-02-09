using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using phaBalloting.Data;
using phaBalloting.Areas.Admin.Models;
using OfficeOpenXml;
using Microsoft.AspNet.Identity;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/Projects
        public ActionResult Index(int? pageNumber)
        {

            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }
            var model = db.Projects.ToList();
            var name= string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower(); 

            //var location= string.IsNullOrEmpty(Request.Form["ProjectLocation"]) ? string.Empty : Request.Form["ProjectLocation"]; ;
            if (name != string.Empty)
            {
                return View(model.Where(x => x.ProjectName.ToLower().Contains(name) || x.Description.ToLower().Contains(name) || x.ProjectLocation.ToLower().Contains(name)  ).ToList().ToPagedList(pageNumber ?? 1, 10));
            }
            
            return View(model.ToPagedList(pageNumber ?? 1, 10));
        }

        // GET: Admin/Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectViewModel project = new ProjectViewModel(id.Value);
            if (project == null)
            {
                return HttpNotFound();
            }



            return View(project);
        }

        // GET: Admin/Projects/Create
        public ActionResult Create()
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            ViewBag.ProjectTypeId = new SelectList(db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted).ToList(), "Id", "TypeName");
            ViewBag.BPS = db.BPSLists.ToList();
            //ViewBag.BPSGroups = new SelectList(db.BPSGroups,"Id","BPSGroup");
            return View();
        }

        // POST: Admin/Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(Project project)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            var bpsSelected = Request.Form["SelectedBPS"];

            project.EntryDate = DateTime.Now;

            project.UserId = User.Identity.GetUserId();
            var attributeids = Request.Form["attributeid"];
            var attributes = Request.Form["attribute"];
            var unitnos = Request.Form["unitNumber"];
            var descriptions = Request.Form["description"];
           
            
            
            if (string.IsNullOrEmpty(unitnos) || string.IsNullOrEmpty(attributeids) || string.IsNullOrEmpty(attributes) )
            {
                ModelState.AddModelError("", "Please fill the attributes of the project accordingly.");
            }
            if (ModelState.IsValid)
            {

                var splittedIds = attributeids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var splittedVals = attributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var splittedunitNumber = unitnos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var splitteddescriptions = descriptions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int splittedunitNumberIndex = 0;
                foreach (var no in splittedunitNumber.Distinct())
                {
                    int unitNumberInt = int.Parse(no.Trim());
                    var projectUnit = new PojectUnit { EntryDate = DateTime.Now, Description = string.Empty, IsActive = true, IsDeleted = false,  UnitNumber = unitNumberInt, UserId = User.Identity.GetUserId()};
                    for (int i = 0; i < splittedIds.Count(); i++)
                    {
                        int unitNumberCurrent = int.Parse(splittedunitNumber[i].Trim());
                        
                        if (unitNumberInt == unitNumberCurrent)
                        {
                            int attributeIdInt = int.Parse(splittedIds[i].Trim());
                            projectUnit.ProjectUnitAttributes.Add(new ProjectUnitAttribute { AttributeId = attributeIdInt, AttributeValue = splittedVals[i], EntryDate = DateTime.Now, IsActive = true, IsDeleted = false, UserId= User.Identity.GetUserId()});
                        }
                    }
                    projectUnit.Description = splitteddescriptions[splittedunitNumberIndex];
                    
                    project.PojectUnits.Add(projectUnit);

                    splittedunitNumberIndex++;
                }
                db.Projects.Add(project);
                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Added Project with Id: " + project.Id + " and Name: " + project.ProjectName);
                    if (!String.IsNullOrEmpty(bpsSelected))
                    {
                        var splittedBPS = bpsSelected.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var bps in splittedBPS)
                        {
                            var bpsId = db.BPSLists.Where(x => x.BPS == bps).ToList();
                            foreach (var b in bpsId)
                            {
                                int a = b.Id;
                                //project.BPSProjects.Add(new BPSProject { ProjectId = project.Id, BPSId = a });
                                db.BPSProjects.Add(new BPSProject { ProjectId = project.Id, BPSId = a });
                            }
                        }
                        db.SaveChanges();
                        UserHelper.WriteActivity("Added Data BPS against Project with Id: " + project.Id);
                    }
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
                catch (Exception a)
                {
                    ModelState.AddModelError("", a.Message);
                }
            }
            string error = "";
            foreach (var state in ModelState)
            {
                error += "<br />"+state.Value;
            }

            ViewBag.ProjectTypeId = new SelectList(db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted).ToList(), "Id", "TypeName");
            ViewBag.BPS = db.BPSLists.ToList();
            return View();
        }
        

        // GET: Admin/Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProjectViewModel project = new ProjectViewModel(id.Value);
            if (project == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProjectTypeId = new SelectList(db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted).ToList(), "Id", "TypeName",project.ProjectTypeId);
            return View(project);
        }

        // POST: Admin/Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProjectViewModel project)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (ModelState.IsValid)
            {
                project.LastModifiedBy = User.Identity.GetUserId();
                project.LastModifiedOn = DateTime.Now;
                db.Entry(project).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();

                    UserHelper.WriteActivity("Modified Project with Id: " + project.Id + " and Name: " + project.ProjectName);
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

            ViewBag.ProjectTypeId = new SelectList(db.ProjectTypes.Where(w => w.IsActive && !w.IsDeleted).ToList(), "Id", "TypeName");
            return View(project);
        }

        // GET: Admin/Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Admin/Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            try
            {
                db.SaveChanges();

                UserHelper.WriteActivity("Deleted Project with Id: " + project.Id + " and Name: " + project.ProjectName);
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

        //Import Excel Function

        public ActionResult ImportExcel()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            if (!EnumManager.Modules.Projects.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }
            
            if (file != null && file.ContentLength > 0)
            {
                if (file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx"))
                {
                    //Read bytes from http input stream
                    System.IO.BinaryReader b = new System.IO.BinaryReader(file.InputStream);
                    //byte[] binData = b.ReadBytes(file.InputStream.Length);
                    var exvelPckg = new OfficeOpenXml.ExcelPackage(file.InputStream).Workbook.Worksheets.FirstOrDefault();
                    var projectName = string.IsNullOrEmpty(exvelPckg.Cells[3, 2].Text.Trim()) ? string.Empty : exvelPckg.Cells[3, 2].Text.ToLower().Trim();
                    var projectExist = db.Projects.ToList().Where(x => x.ProjectName.ToLower().Trim() == projectName).FirstOrDefault();
                    if (projectExist == null)
                    {
                        ModelState.AddModelError("","Project Name is invalid. No such record found.");
                        return View();
                    }

                    for (int i = 3; i <= exvelPckg.Dimension.End.Column; i++)
                    {
                        string colName = exvelPckg.Cells[2, i].Value.ToString();
                        if (string.IsNullOrEmpty(colName))
                            continue;
                        var attributeInDb = db.AttributesTypes.Where(w => w.AttributeName == colName).FirstOrDefault();
                        if (attributeInDb == null)
                        {
                            attributeInDb = new AttributesType();
                            attributeInDb.AttributeName = colName;
                            attributeInDb.DataTypeId = 1;
                            attributeInDb.Description = string.Empty;
                            attributeInDb.EntryDate = DateTime.Now;
                            attributeInDb.IsActive = true;
                            attributeInDb.IsDeleted = false;
                            attributeInDb.UserId = User.Identity.GetUserId();
                            attributeInDb.ProjectTypeConfigurations.Add(new ProjectTypeConfiguration { Description = string.Empty, EntryDate = DateTime.Now, IsActive = true, IsDeleted = false, PojectTypeId = projectExist.ProjectTypeId.Value, UserId = User.Identity.GetUserId() });
                            db.AttributesTypes.Add(attributeInDb);
                            db.SaveChanges();
                        }
                    }

                    for (int row = 3; row <= exvelPckg.Dimension.End.Row; row++)
                    {
                        
                        PojectUnit projectUnit = null;
                        if (row > 1)
                        {
                            new PojectUnit();
                            projectUnit = new PojectUnit { PojectId = projectExist.Id, EntryDate = DateTime.Now, Description = string.Empty, IsActive = true, IsDeleted = false, UnitNumber = int.Parse(exvelPckg.Cells[row, 1].Value.ToString().Trim()), UserId = User.Identity.GetUserId() };

                        }
                        if (projectUnit != null)
                        {
                            //first 3 columns are S.no, ProjectName, Project Types
                            for (int col = 3; col <= exvelPckg.Dimension.End.Column; col++)
                            {
                                string colName = exvelPckg.Cells[2, col].Value.ToString();
                                if (string.IsNullOrEmpty(colName))
                                    continue;
                                var attributeInDb = db.AttributesTypes.Where(w => w.AttributeName == colName).FirstOrDefault();


                                if (attributeInDb != null)
                                {
                                    ProjectUnitAttribute unitAttribute = new ProjectUnitAttribute();
                                    unitAttribute.AttributeId = attributeInDb.Id;
                                    unitAttribute.AttributeValue = string.IsNullOrEmpty(exvelPckg.Cells[row, col].Text.Trim()) ? string.Empty : exvelPckg.Cells[row, col].Text.ToLower().Trim();
                                    unitAttribute.IsActive = true;
                                    unitAttribute.IsDeleted = false;
                                    unitAttribute.EntryDate = DateTime.Now;
                                    unitAttribute.UserId = User.Identity.GetUserId();

                                    //unitAttribute.UnitId = db.PojectUnits.ToList().LastOrDefault().Id;

                                    projectUnit.ProjectUnitAttributes.Add(unitAttribute);
                                }
                                else
                                {
                                    ModelState.AddModelError("","No Project Attribute Defined for "+colName);
                                    return View();
                                }
                            }//end of column loop
                            db.PojectUnits.Add(projectUnit);
                        }//end of projectUnit!=null
                    }//end of row loop
                    #region SumairaMethod
                    /*
                     * var start = exvelPckg.Dimension.Start;
                    var end = exvelPckg.Dimension.End;
                    
                    string state="continue";

                    List<string> ColumnNames = new List<string>();
                    for (int i = 2; i <= end.Column; i++)
                    {
                        ColumnNames.Add(exvelPckg.Cells[1, i].Value.ToString());
                    }

                    int projectId = 0;
                    
                        for (int row = start.Row + 1; row <= end.Row; row++)
                    {
                        if (state.Equals("continue"))
                            for (int col = start.Column + 1; col <= end.Column; col++)
                            {
                                if (state.Equals("continue"))
                                    foreach (var colName in ColumnNames)
                                    {
                                        if (col == 2 && row > 1)
                                        {
                                            //projectUnit = new PojectUnit { EntryDate = DateTime.Now, Description = string.Empty, IsActive = true, IsDeleted = false, UnitNumber = int.Parse(exvelPckg.Cells[row, col].Value.ToString().Trim()), UserId = User.Identity.GetUserId() };

                                        }
                                        if (colName == "Project" && col == 3)
                                        {
                                            var value = string.IsNullOrEmpty(exvelPckg.Cells[row, col].Text.Trim()) ? string.Empty : exvelPckg.Cells[row, col].Text.Trim();
                                            var projectExist = db.Projects.Where(x => x.ProjectName == value).FirstOrDefault();

                                            if (projectExist == null)
                                            {
                                                state = "break";
                                                break;
                                            }
                                            else
                                            {
                                                projectId = projectExist.Id;

                                                projectUnit.PojectId = projectId;
                                                db.PojectUnits.Add(projectUnit);
                                                try { db.SaveChanges(); }
                                                catch (Exception ex) { string excep = ex.Message; }
                                            }
                                        }
                                        else if (colName != "Project" && colName != "S. No." && col > 3)
                                        {
                                            var attributeInDb = db.AttributesTypes.Where(w => w.AttributeName == colName).FirstOrDefault();
                                            if (attributeInDb != null)
                                            {
                                                ProjectUnitAttribute unitAttribute = new ProjectUnitAttribute();
                                                unitAttribute.AttributeId = attributeInDb.Id;
                                                unitAttribute.AttributeValue = exvelPckg.Cells[row, col].Value.ToString();
                                                
                                                unitAttribute.IsActive = true;
                                                unitAttribute.IsDeleted = false;
                                                unitAttribute.EntryDate = DateTime.Now;
                                                unitAttribute.UserId = User.Identity.GetUserId();

                                                unitAttribute.UnitId = db.PojectUnits.ToList().LastOrDefault().Id;

                                                db.ProjectUnitAttributes.Add(unitAttribute);
                                                
                                            }
                                            else
                                            {
                                                state = "break";
                                                break;
                                            }

                                        }
                                    }
                                else
                                    break;
                                
                            }
                        else
                        {
                            ModelState.AddModelError("", "Some of attribute in excelsheet does not exist in database. Please add all attributes of project and then upload.");
                            break;
                        }
                    }
                        */
                    #endregion
                    try {
                        db.SaveChanges();
                        ViewBag.Sucess = "Successfully Imported Records."; 
                    }
                    catch  { ModelState.AddModelError("","Unable to Store data. Please verify the excel sheet format and data.");
                        return View();
                    }
                    UserHelper.WriteActivity("Imported Excel File of Project : " + exvelPckg.Cells[1,2].Value.ToString());
                    return RedirectToAction("Index");
                    
                }
            }
            return View();
        }
    }
}
