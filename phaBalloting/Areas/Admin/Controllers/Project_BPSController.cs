using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using phaBalloting.Data;
using phaBalloting.Helpers;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class Project_BPSController : Controller
    {
        private phaEntities db = new phaEntities();
        // GET: Admin/Project_BPS
        public ActionResult Index()
        {
            

            ViewBag.PId = new SelectList(db.Projects, "Id", "ProjectName");
            return View(db.BPSLists.ToList());
        }
        [HttpPost]
        public ActionResult Index(int projectId, List<BPSList> bpsSelectedList)
        {
            ViewBag.PId = new SelectList(db.Projects, "Id", "ProjectName");
            if (string.IsNullOrEmpty(Request.Form["PId"]))
            {
                ModelState.AddModelError("PId", "Please Select a project to continue.");
            }


            if (string.IsNullOrEmpty(Request.Form["BPS"]))
            {
                ModelState.AddModelError("BPS", "Please Select a BPS to continue.");
            }

            int id = int.Parse(Request.Form["PId"]);

            //var bpss = bPS;

            var selectedBpss = Request.Form["BPS"];
            string[] splittedBPs = selectedBpss.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < splittedBPs.Length; i++)
            {
                string[] scaleBps;
                scaleBps = splittedBPs[i].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                int bps = int.Parse(scaleBps[0]);
                foreach (var bpss in scaleBps)
                {
                    //for(int x=j;x<scaleBps.)
                    while (bps <= int.Parse(bpss))
                    {
                        try
                        {
                            BPSProject bpsp = new BPSProject();
                            bpsp.ProjectId = id;
                            bpsp.BPSId = bps;


                            db.BPSProjects.Add(bpsp);
                            bps++;
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                    }
                }
            }

            try
            {
                db.SaveChanges();

                UserHelper.WriteActivity("BPS Added against Project Id: " + id );
                return RedirectToAction("ShowRecords");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException invalids)
            {

                foreach (var invalid in invalids.EntityValidationErrors)
                {
                    foreach (var field in invalid.ValidationErrors)
                    {

                        ModelState.AddModelError(field.PropertyName, field.ErrorMessage);
                    }
                }
            }
            catch (Exception aa)
            {
                ModelState.AddModelError("", aa.Message);
            }

            
            //return View(db.BPSLists.ToList());

            return View(db.BPSLists.ToList());
        }
        public ActionResult ShowRecords()
        {
            return View(db.BPSProjects.ToList());
        }
    }
}