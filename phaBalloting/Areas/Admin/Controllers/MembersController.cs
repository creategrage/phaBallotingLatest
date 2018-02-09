using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using PagedList;
using System.Data.Entity.Validation;
using System.Web.Mvc;
using phaBalloting.Data;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Drawing;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private phaEntities db = new phaEntities();

        // GET: Admin/Members
        public ActionResult Index(int? pageNumber)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }

            List<Member> model = new List<Member>();
            var name = string.IsNullOrEmpty(Request["keywords"]) ? string.Empty : Request["keywords"].ToLower(); 
            if (name != string.Empty)
            {
                model = db.Members.Where(w => w.IsActive && !w.IsDeleted).ToList().Where(w=>w.Cnic.ToLower().Contains(name) || w.EmailAddress.ToLower().Contains(name) || w.FatherName.ToLower().Contains(name)|| w.HomeTelephone.ToLower().Contains(name) || w.HusbandName.ToLower().Contains(name) || w.NameOfOfficer.ToLower().Contains(name) || w.OfficeAddress.ToLower().Contains(name) || w.OfficeStatus.ToLower().Contains(name) || w.OfficeTelephone.ToLower().Contains(name) || w.OldMFormNo.ToLower().Contains(name) || w.PermanentAddress.ToLower().Contains(name) || w.PostHeld.ToLower().Contains(name) ).ToList();
            }
            
            else
            {
                model = db.Members.Where(w => w.IsActive && !w.IsDeleted).ToList();
            }

            return View(model.ToPagedList(pageNumber ?? 1, 10));
        }

        // GET: Admin/Members/Details/5
        public ActionResult Details(int? id)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        // GET: Admin/Members/Create
        public ActionResult Create()
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            ViewBag.BPS= new SelectList(db.BPSLists, "Id", "BPS");

            return View();
        }

        // POST: Admin/Members/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Member member, HttpPostedFileBase ImageUrl)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }

            if (ModelState.IsValid)
            {
                member.IsActive = true;
                member.IsDeleted = false;
                member.UserId = User.Identity.GetUserId();
                member.EntryDate = DateTime.Now;
                member.BPSId = int.Parse(Request.Form["BPS"]);
                member.ImageUrl = SaveMemberPic(ImageUrl, member.Cnic);
                db.Members.Add(member);

                try
                {
                    db.SaveChanges();
                    UserHelper.WriteActivity("Added Member with Id: " + member.Id + "Name: " + member.NameOfOfficer);
                }
                catch (Exception ex)
                { ex.ToString(); }
                return RedirectToAction("Index");
            }

            return View(member);
        }

        // GET: Admin/Members/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            ViewBag.BPS = new SelectList(db.BPSLists, "Id", "BPS");
            return View(member);
        }

        // POST: Admin/Members/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OldMFormNo,NameOfOfficer,FatherName,HusbandName,Cnic,OfficeName,OfficeStatus,DateOfJoiningService,PostHeld,OccutionalGroup,DateOfBirth,OfficeAddress,PermanentAddress,HomeTelephone,EmailAddress,ImageUrl,OfficeTelephone,Mobile")] Member member, HttpPostedFileBase NewImage)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (ModelState.IsValid)
            {
                member.ModifiedBy = User.Identity.GetUserId();
                member.ModfiedOn= DateTime.Now;
                member.ImageUrl = SaveMemberPic(NewImage, member.Cnic);
                db.Entry(member).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();

                    UserHelper.WriteActivity("Edited Member with id " + member.Id + " Name: " + member.NameOfOfficer);
                }
                catch { }
                return RedirectToAction("Index");
            }
            return View(member);
        }

        // GET: Admin/Members/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        // POST: Admin/Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EnumManager.Modules.Members.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            Member member = db.Members.Find(id);
            db.Members.Remove(member);
            try
            {
                db.SaveChanges();
                UserHelper.WriteActivity("Deleted Member with id " + id + " Name: " + member.NameOfOfficer);
            }
            catch { }
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

            DataSet ds = new DataSet();
            DataTable datatab = new DataTable();

            if (file != null && file.ContentLength > 0)
            {
                if (file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx"))
                {
                    FileInfo f = new FileInfo(file.FileName);
                    
                    var exvelPckg = new OfficeOpenXml.ExcelPackage(f, true).Workbook.Worksheets.FirstOrDefault();

                    var start = exvelPckg.Dimension.Start;
                    var end = exvelPckg.Dimension.End;
                    
                    datatab = WorksheetToDataTable(exvelPckg.Workbook.Worksheets.FirstOrDefault());

                    int result = 0;

                    string ErrorPath = Server.MapPath("~/Content/") + DateTime.Now.ToString("yyyyMMddhhmmss") + System.IO.Path.GetFileNameWithoutExtension(Request.Files["file"].FileName) + ".txt";
                    StreamWriter sw = new StreamWriter(ErrorPath, true);

                    for (int i = start.Row; i < end.Row; i++)
                    {

                        DataRow crow = datatab.Rows[i];
                        

                        Member member = new Member();

                        if (crow[0].ToString().Trim() != "n/a")
                        {
                            if (int.TryParse(crow[0].ToString().Trim(), out result) && (i >= 3))
                            {

                                member.NameOfOfficer = string.IsNullOrEmpty(crow[1].ToString().Trim()) ? string.Empty : crow[1].ToString().Trim();
                                member.FatherName = string.IsNullOrEmpty(crow[2].ToString().Trim()) ? string.Empty : crow[2].ToString().Trim().Trim();
                                //member.HusbandName = string.IsNullOrEmpty(crow[2].ToString().Trim()) ? string.Empty : crow[2].ToString().Trim();
                                member.Cnic = string.IsNullOrEmpty(crow[3].ToString().Trim()) ? string.Empty : crow[3].ToString().Trim();

                                string p = crow[4].ToString().Trim();
                                BPSList bpsl = db.BPSLists.Where(x => x.BPS == p).FirstOrDefault();
                                if (bpsl == null)
                                {
                                    BPSList bps = new BPSList();
                                    bps.BPS = string.IsNullOrEmpty(crow[4].ToString().Trim()) ? string.Empty : crow[4].ToString().Trim();
                                    bps.IsActive = true;
                                    bps.EntryDate = DateTime.Now;
                                    bps.IsDeleted = false;
                                    bps.UserId = User.Identity.GetUserId();
                                    db.BPSLists.Add(bps);
                                    try { db.SaveChanges(); }
                                    catch { }

                                    member.BPSId = bps.Id;
                                }
                                else { member.BPSId = bpsl.Id; }

                                member.OldMFormNo = string.IsNullOrEmpty(crow[5].ToString().Trim()) ? string.Empty : crow[5].ToString().Trim();
                                member.OccutionalGroup = string.IsNullOrEmpty(crow[6].ToString().Trim()) ? string.Empty : crow[6].ToString().Trim();
                                member.OfficeName = string.IsNullOrEmpty(crow[7].ToString().Trim()) ? string.Empty : crow[7].ToString().Trim();
                                member.OfficeStatus = string.IsNullOrEmpty(crow[8].ToString().Trim()) ? string.Empty : crow[8].ToString().Trim();

                                member.PermanentAddress = string.IsNullOrEmpty(crow[13].ToString().Trim()) ? string.Empty : crow[13].ToString().Trim();
                                member.OfficeAddress = string.IsNullOrEmpty(crow[14].ToString().Trim()) ? string.Empty : crow[14].ToString().Trim();
                                member.OfficeTelephone = string.IsNullOrEmpty(crow[15].ToString().Trim()) ? string.Empty : crow[15].ToString().Trim();
                                member.HomeTelephone = string.IsNullOrEmpty(crow[16].ToString().Trim()) ? string.Empty : crow[16].ToString().Trim();
                                member.Mobile = string.IsNullOrEmpty(crow[17].ToString().Trim()) ? string.Empty : crow[17].ToString().Trim();

                                member.ImageUrl = string.Empty;

                                member.DateOfBirth = DateTime.Now; 

                                member.DateOfJoiningService = DateTime.Now; 

                                member.EmailAddress = string.Empty;

                                member.EntryDate = DateTime.Now; 

                                member.HusbandName = string.Empty;
                                member.IsActive = true;
                                member.IsDeleted = false;
                                member.ModfiedOn = DateTime.Now; 
                                member.UserId = User.Identity.GetUserId();
                                member.ModifiedBy = User.Identity.GetUserId();
                                member.PostHeld = string.Empty;

                                db.Members.Add(member);
                                db.SaveChanges();
                            }
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                        UserHelper.WriteActivity("Uploaded Member File in Database.");
                        ModelState.AddModelError("FormMessage", "Data is successfully saved.");
                        return RedirectToAction("Index");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var entityvalidation in ex.EntityValidationErrors)
                        {
                            foreach (var vri in entityvalidation.ValidationErrors)
                            {
                                ModelState.AddModelError("FormError", "property: " + vri.PropertyName + " Error: " + vri.ErrorMessage);
                            }

                        }
                    }
                    catch (Exception ex) { string message = ex.Message; }
                }
            }
            return View();
        }
        private DataTable WorksheetToDataTable(OfficeOpenXml.ExcelWorksheet oSheet)
        {
            int totalRows = oSheet.Dimension.End.Row;
            int totalCols = oSheet.Dimension.End.Column;
            DataTable dt = new DataTable(oSheet.Name);
            DataRow dr = null;
            for (int i = 1; i <= totalRows; i++)
            {
                if (i > 1) dr = dt.Rows.Add();
                for (int j = 1; j <= totalCols; j++)
                {
                    if (i == 1)
                        try
                        {
                            dt.Columns.Add(j.ToString(), typeof(string));
                        }
                        catch { }
                    else
                        try
                        {
                            dr[j - 1] = oSheet.Cells[i, j].Value.ToString();
                        }
                        catch
                        {
                            try
                            {
                                dr[j - 1] = "n/a";
                            }
                            catch { }
                        }
                }
            }
            return dt;
        }
        public string SaveMemberPic(HttpPostedFileBase ImageFile,string filename)
        {
            string path = "~/Areas/Admin/assets/MemberPhotos/";
            if (ImageFile != null)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(ImageFile.FileName);
                    var fullPath = System.IO.Path.Combine(Server.MapPath(path), filename + extension);
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                    ImageFile.SaveAs(fullPath);
                    path = path + filename  + extension;
                }
                catch
                {
                    path = string.Empty;
                }
            }
            else path = string.Empty;
            
            return path;
        }
    }
}
