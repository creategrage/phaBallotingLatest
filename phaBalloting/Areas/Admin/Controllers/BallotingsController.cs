using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using phaBalloting.Data;
using PagedList;
using phaBalloting.Areas.Admin.Models;
using phaBalloting.Helpers;


namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class BallotingsController : Controller
    {
        private phaEntities db = new phaEntities();
        private BallotingModel ballotModel = new BallotingModel();

        // GET: Admin/Ballotings
        public ActionResult Index(int? pageNumber)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }
            var ballotings = db.Ballotings.Include(b => b.Event).Include(b => b.Member).Include(b => b.PojectUnit).ToList().OrderBy(x=>x.Id);
            var name = string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower();

            //var location= string.IsNullOrEmpty(Request.Form["ProjectLocation"]) ? string.Empty : Request.Form["ProjectLocation"]; ;
            if (name != string.Empty)
            {
                return View(ballotings.Where(x => x.Member.NameOfOfficer.ToLower().Contains(name) || x.Event.EventName.ToLower().Contains(name) || x.PojectUnit.Project.ProjectName.ToLower().Contains(name)).ToList().ToPagedList(pageNumber ?? 1, 10));
            }

            return View(ballotings.ToPagedList(pageNumber ?? 1, 10));
            
        }

        // GET: Admin/Ballotings/Details/5
        public ActionResult Details(int? id)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Balloting balloting = db.Ballotings.Find(id);
            if (balloting == null)
            {
                return HttpNotFound();
            }
            return View(balloting);
        }

        // GET: Admin/Ballotings/Create
        public ActionResult Create()
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            ViewBag.ProjectID = new SelectList(db.Projects, "Id", "ProjectName");

            ViewBag.EventID = new SelectList(db.Events, "Id", "EventName");
            ViewBag.MemberID = new SelectList(db.Members, "Id", "OldMFormNo");
            ViewBag.ProjectUnitID = new SelectList(db.PojectUnits, "Id", "Description");
            return View();
        }

        // POST: Admin/Ballotings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "Id,PUID,MID,EID")] Balloting balloting)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }

            int projectId = int.Parse(Request.Form["ProjectID"]);
            var waitingPercent = db.phaConfigurations.Where(w => w.ConfigKey == "PercentageToBeWaited").FirstOrDefault();
            if (waitingPercent == null) {
                throw new NullReferenceException();
            }


            double percent = double.Parse(waitingPercent.ConfigValue);
            double percentage = percent / 100 ;
            int eventId = int.Parse(Request.Form["EventID"]);
            
            IEnumerable<PojectUnit> projunitList = db.PojectUnits.Where(x => x.Project.Id == projectId || projectId == 0).ToList();
            var projectBPSGroup = db.BPSProjects.Where(x => x.ProjectId == projectId).ToList();
            List<Member> membersList = new List<Member>();

            foreach (var bps in projectBPSGroup)
            {
                List<Member> members = db.Members.Where(x => x.BPSList.Id == bps.BPSId).ToList();
                foreach (var member in members)
                {
                    membersList.Add(member);
                }
            }

            if (ballotModel.AllocationBallots(membersList, projunitList, percentage, eventId))
            {
                
                var ballotings = db.Ballotings.Include(b => b.Member).Include(b => b.PojectUnit);
                UserHelper.WriteActivity("Added Ballot in Project ID: " + projectId+ "against Event Id: " + eventId);
                return View("Index", ballotings);
            }

            
            //ViewBag.EventID = events;
            ViewBag.EventID = new SelectList(db.Events, "Id", "EventName", balloting.EventID);
            ViewBag.MemberID = new SelectList(db.Members, "Id", "OldMFormNo", balloting.MemberID);
            ViewBag.ProjectUnitID = new SelectList(db.PojectUnits, "Id", "Description", balloting.ProjectUnitID);

            return View(balloting);
        }

        // GET: Admin/Ballotings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Balloting balloting = db.Ballotings.Find(id);
            if (balloting == null)
            {
                return HttpNotFound();
            }
            ViewBag.EID = new SelectList(db.Events, "Id", "EventName", balloting.EventID);
            ViewBag.MID = new SelectList(db.Members, "Id", "OldMFormNo", balloting.MemberID);
            ViewBag.PUID = new SelectList(db.PojectUnits, "Id", "Description", balloting.ProjectUnitID);
            return View(balloting);
        }

        // POST: Admin/Ballotings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Balloting balloting)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.EditRecords))
            {
                return View("NotAuthorize");
            }

            if (ModelState.IsValid)
            {
                db.Entry(balloting).State = EntityState.Modified;
                db.SaveChanges();
                UserHelper.WriteActivity("Edited Ballot in Project ID: " + balloting.PojectUnit.PojectId+ "against Event Id: " +balloting.EventID);
                return RedirectToAction("Index");
            }
            ViewBag.EID = new SelectList(db.Events, "Id", "EventName", balloting.EventID);
            ViewBag.MID = new SelectList(db.Members, "Id", "OldMFormNo", balloting.MemberID);
            ViewBag.PUID = new SelectList(db.PojectUnits, "Id", "Description", balloting.ProjectUnitID);
            return View(balloting);
        }

        // GET: Admin/Ballotings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Balloting balloting = db.Ballotings.Find(id);
            if (balloting == null)
            {
                return HttpNotFound();
            }
            return View(balloting);
        }

        // POST: Admin/Ballotings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.DeleteRecords))
            {
                return View("NotAuthorize");
            }

            Balloting balloting = db.Ballotings.Find(id);
            db.Ballotings.Remove(balloting);
            db.SaveChanges();
            UserHelper.WriteActivity("Deleted Ballot of Project ID: " + balloting.PojectUnit.PojectId + "against Event Id: " + balloting.EventID);
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
