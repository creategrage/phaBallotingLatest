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
        public ActionResult Index()
        {
            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.ViewRecords))
            {
                return View("NotAuthorize");
            }
            ViewBag.ProjectID = new SelectList(db.Projects, "Id", "ProjectName");

            ViewBag.EventID = new SelectList(db.Events, "Id", "EventName");

            //var ballotings = db.Ballotings.Include(b => b.Event).Include(b => b.Member).Include(b => b.PojectUnit).ToList().OrderBy(x=>x.Id);
            //var name = string.IsNullOrEmpty(Request.Form["keywords"]) ? string.Empty : Request.Form["keywords"].ToLower();

            ////var location= string.IsNullOrEmpty(Request.Form["ProjectLocation"]) ? string.Empty : Request.Form["ProjectLocation"]; ;
            //if (name != string.Empty)
            //{
            //    return View(ballotings.Where(x => x.Member.NameOfOfficer.ToLower().Contains(name) || x.Event.EventName.ToLower().Contains(name) || x.PojectUnit.Project.ProjectName.ToLower().Contains(name)).ToList().ToPagedList(pageNumber ?? 1, 10));
            //}
            //ballotings.ToPagedList(pageNumber ?? 1, 10)
            return View(new DataTable());
            
        }

        [HttpPost]
        public ActionResult Index(int EventId,int ProjectId, int? pageNumber)
        {
            //PagedList<DataRow> list = new PagedList<DataRow>(table.AsEnumerable(), pageNumber.Value, 20);
            string keywords = Request["keywords"];
            string field = Request["field"];
            string filters = string.Empty;
            if(!string.IsNullOrEmpty(field) && !string.IsNullOrEmpty(keywords))
            {
                filters =" "+ field  + " Like '%" + keywords + "%'";
                UserHelper.WriteActivity("Searched balloting for Member: " + field + " Project: " + keywords);

            }
            DataTable table = new BallotingFinalResults(EventId, ProjectId,0,filters).GetData();
            ViewBag.ProjectID = new SelectList(db.Projects, "Id", "ProjectName");

            ViewBag.EventID = new SelectList(db.Events, "Id", "EventName");

            return View(table);
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
            return View();
        }

        
        // POST: Admin/Ballotings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(int ProjectID, int EventID )
        {

            if (!EnumManager.Modules.Balloting.IsAuthrozed(EnumManager.Actions.AddRecords))
            {
                return View("NotAuthorize");
            }
            if (Request.Form["BallotingType"] == null || string.IsNullOrEmpty(Request.Form["BallotingType"]))
            {
                ModelState.AddModelError("", "Invalid balloting type selected for balloting.");
                return View();
            }

            string ballotingType = Request.Form["BallotingType"];
            List<string> ballotingTypes = new List<string> { EnumManager.ballotinTypes.run.ToString(), EnumManager.ballotinTypes.rerun.ToString(), EnumManager.ballotinTypes.runwaiting.ToString()};
            if (!ballotingTypes.Contains(ballotingType))
            {
                ModelState.AddModelError("", "Invalid Balloting type selected for balloting.");
                return View();
            }

            EnumManager.ballotinTypes currentBallotingType = (EnumManager.ballotinTypes)Enum.Parse(typeof(EnumManager.ballotinTypes), ballotingType);

            int projectId = int.Parse(Request.Form["ProjectID"]);
            
            var CurrentProject = db.Projects.Where(w => w.Id == projectId).FirstOrDefault();
            if (CurrentProject == null)
            {
                ModelState.AddModelError("","Invalid Project selected for balloting.");
               
            }
            int eventId = int.Parse(Request.Form["EventID"]);
            var CurrentEvent = db.Events.Where(w => w.Id == eventId).FirstOrDefault();
            if (CurrentEvent == null)
            {
                ModelState.AddModelError("", "Invalid Event selected for balloting.");
                
            }

            List<PojectUnit> projunitList = new List<PojectUnit>();


            if (currentBallotingType == EnumManager.ballotinTypes.run || currentBallotingType == EnumManager.ballotinTypes.rerun)
                projunitList = db.PojectUnits.Where(x => x.Project.Id == projectId).ToList();
            else if (currentBallotingType == EnumManager.ballotinTypes.runwaiting)
                projunitList = db.PojectUnits.Where(x => x.Project.Id == projectId && x.Ballotings.Where(b => b.CancelledBallotings.Count > 0 && b.EventID == eventId).Count() > 0 && x.Ballotings.Where(b => b.CancelledBallotings.Count == 0 && b.EventID == eventId).Count() == 0).ToList();

            if (projunitList.Count() == 0)
            {
                ModelState.AddModelError("", "No Applicable Units found in Selected Project.");
            }
            var projectBPSGroup = db.BPSProjects.Where(x => x.ProjectId == projectId).ToList().Select(s => s.BPSList.BPS.ToLower().Trim()).ToList();
            if(projectBPSGroup.Count==0)
            {
                ModelState.AddModelError("", "No BPS Included for Selected Project.");
            }
            List<Member> membersToBeIncluded = new List<Member>();
            if (currentBallotingType == EnumManager.ballotinTypes.run || currentBallotingType == EnumManager.ballotinTypes.rerun)
                membersToBeIncluded = db.Members.Where(w => w.Ballotings.Count() == 0 && w.IsActive && !w.IsDeleted).ToList().Where(w => projectBPSGroup.Contains(w.BPSList.BPS.Trim().ToLower())).ToList();
            else if (currentBallotingType == EnumManager.ballotinTypes.runwaiting)
                membersToBeIncluded = db.Members.Where(w => w.Ballotings.Count() == 0 && w.WaitingMembers.Where(ww => ww.ProjectID == projectId && ww.EventID == eventId).Count() > 0 && w.IsActive && !w.IsDeleted).ToList().Where(w => projectBPSGroup.Contains(w.BPSList.BPS.Trim().ToLower())).ToList();

            if (membersToBeIncluded.Count == 0)
            {
                ModelState.AddModelError("", "No Members found for Selected Project.");
            }
           

            var waitingPercent = db.phaConfigurations.Where(w => w.ConfigKey == "PercentageToBeWaited").FirstOrDefault();
            if (waitingPercent == null) {
                ModelState.AddModelError("", "Percentage for waiting list is not defined.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            double percent = double.Parse(waitingPercent.ConfigValue);
            double percentage = percent / 100 ;

            if (currentBallotingType == EnumManager.ballotinTypes.run || currentBallotingType == EnumManager.ballotinTypes.rerun)
            {
                var existings = db.Ballotings.Where(w => w.PojectUnit.PojectId == projectId && w.EventID == EventID).Select(s => s.Id).ToArray();
                if (existings.Count() > 0)
                {
                    db.Ballotings.RemoveRange(db.Ballotings.Where(r => existings.Contains(r.Id)));
                    try
                    {
                        db.SaveChanges();
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Unable to remove previous balloting results.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                if (ballotModel.AllocationBallots(membersToBeIncluded, projunitList, percentage, eventId))
                {
                    UserHelper.WriteActivity("Performed balloting for Project: " + CurrentProject.ProjectName + " Event: " + CurrentEvent.EventName);

                    DataTable table = new BallotingFinalResults(CurrentEvent.Id, CurrentProject.Id).GetData();

                    return View("Index", table);
                }
            }

            
            //ViewBag.EventID = events;
            ViewBag.EventID = new SelectList(db.Events, "Id", "EventName", eventId);
            ViewBag.ProjectID = new SelectList(db.Projects, "Id", "ProjectName", projectId);


            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string CancelBalloting(int ballotingId,string description)
        {
            Data.phaEntities db = new Data.phaEntities();
            var CancelledBalloting = db.CancelledBallotings.Where(w => w.BallotingId == ballotingId).FirstOrDefault();
            if (CancelledBalloting != null)
            {
                return "already cancelled";
            }
            CancelledBalloting = new Data.CancelledBalloting() { Description = description, BallotingId = ballotingId, EntryDate = DateTime.Now, UserId = User.Identity.GetUserId() };
            try
            {
                string membername = CancelledBalloting.Balloting.Member.NameOfOfficer;
                string projectname = CancelledBalloting.Balloting.PojectUnit.Project.ProjectName;
                string Id = CancelledBalloting.Id.ToString();
                db.CancelledBallotings.Add(CancelledBalloting);
                db.SaveChanges();
                UserHelper.WriteActivity("ID:" + Id + " Canceled balloting for Member: " + membername + " Project: " + projectname);

                return "sucess";
            }
            catch
            {
                return "failed";
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
