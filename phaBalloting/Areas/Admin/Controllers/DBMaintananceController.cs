using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace phaBalloting.Areas.Admin.Controllers
{
    public class DBMaintananceController : Controller
    {
        // GET: Admin/DBMaintanance
        public ActionResult Backup()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Backup")]
        public string BackupData()
        {
            if (Helpers.DBMaintanance.Backup(Server.MapPath("~/Content/Backup/DatabaseBackup_" + DateTime.Now.ToString("dd_MMM_yyyy_hh_mm_tt") + ".bak")))
                return "successfully created backup of database with Name: DatabaseBackup_" + DateTime.Now.ToString("dd_MMM_yyyy_hh_mm_tt") + ".bak";

            return "unable to create backup of database";
        }

        public ActionResult Restore()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Restore")]
        public bool RestoreData(string path)
        {
            Helpers.DBMaintanance.Restore(Server.MapPath("~/Content/Backup/"+path));
            return false;
        }
    }
}