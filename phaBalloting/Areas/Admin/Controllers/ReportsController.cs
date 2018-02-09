using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;

using System.Data;
namespace phaBalloting.Areas.Admin.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Admin/Reports
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Reports
        public FileResult SuccessFullMembers(int eventid = 0, int projectId = 0, int memberId = 0,string reportType="success")
        {
            DataTable table = new phaBalloting.Areas.Admin.Models.BallotingFinalResults(eventid, projectId, memberId,"",reportType).GetData();
            if (table.Columns.Contains("Floor")) table.Columns["Floor"].ColumnName = "c1";
            if (table.Columns.Contains("Flat No")) table.Columns["Flat No"].ColumnName = "c2";

            if (table.Columns.Contains("OldMFormNo")) table.Columns["OldMFormNo"].ColumnName = "MemberNo";

            if (table.Columns.Contains("NameOfOfficer")) table.Columns["NameOfOfficer"].ColumnName = "MemberName";
            string title = reportType == "success" ? "List of Successfull Members" : "List of Cancelled Members";
            var reportViewModel = new phaBalloting.Areas.Admin.Models.ReportViewModel()
            {
                FileName = "~/Content/reports/SuccessfullReport.rdlc",
                //LeftMainTitle = "ABC Company Name",
                //LeftSubTitle = "DEF Department Name",
                //RightMainTitle = "اسم الشركة",
                //RightSubTitle = "اسم القسم",
                Name = "Statistical Report",
                //ReportDate = DateTime.Now,
                Logo = "~/Content/logo.jpg",
                ReportTitle = title,
                SubTitle = " ",
                Contact = " ",
                ReportLanguage = "en-US",
                Format = phaBalloting.Areas.Admin.Models.ReportViewModel.ReportFormat.PDF,
                UserNamPrinting = "",
                ViewAsAttachment = false,
            };
            //adding the dataset information to the report view model object
            reportViewModel.ReportDataSets.Add(new phaBalloting.Areas.Admin.Models.ReportViewModel.ReportDataSet() { Data = table, DatasetName = "DataSet1" });
            return File(reportViewModel.RenderReport(), reportViewModel.LastmimeType);

        }

        public FileResult FailureList(int eventid = 0, int projectId = 0)
        {
            var db = new Data.phaEntities();
             var events = db.Events.Where(w => w.Id == eventid).FirstOrDefault();

            var project = db.Projects.Where(w => w.Id == projectId).FirstOrDefault();


            var data = db.Members.Where(w => w.Ballotings.Count() == 0).ToList().Select(s => new { MemberNo = s.OldMFormNo, MemberName = s.NameOfOfficer,CNIC= s.Cnic, s.BPSList.BPS, EventName = events.EventName, EventLocation = events.EventLocation, HeldDate = events.EntryDate, ProjectName = project.ProjectName, ProjectLocation = project.ProjectLocation, ProjectDescription = project.Description });
            
            var reportViewModel = new phaBalloting.Areas.Admin.Models.ReportViewModel()
            {
                FileName = "~/Content/reports/MemberReport.rdlc",
                //LeftMainTitle = "ABC Company Name",
                //LeftSubTitle = "DEF Department Name",
                //RightMainTitle = "اسم الشركة",
                //RightSubTitle = "اسم القسم",
                Name = "Statistical Report",
                //ReportDate = DateTime.Now,
                Logo = "~/Content/logo.jpg",
                ReportTitle = "List of Failure Members",
                SubTitle = " ",
                Contact = " ",
                ReportLanguage = "en-US",
                Format = phaBalloting.Areas.Admin.Models.ReportViewModel.ReportFormat.PDF,
                UserNamPrinting = "",
                ViewAsAttachment = false,
            };
            //adding the dataset information to the report view model object
            reportViewModel.ReportDataSets.Add(new phaBalloting.Areas.Admin.Models.ReportViewModel.ReportDataSet() { Data = data, DatasetName = "DataSet1" });
            return File(reportViewModel.RenderReport(), reportViewModel.LastmimeType);

        }

        public FileResult WaitingList(int eventid = 0, int projectId = 0)
        {
            var db = new Data.phaEntities();
            var events = db.Events.Where(w => w.Id == eventid).FirstOrDefault();

            var project = db.Projects.Where(w => w.Id == projectId).FirstOrDefault();


            var data = db.Members.Where(w => w.Ballotings.Count() == 0).ToList().Select(s => new { MemberNo = s.OldMFormNo, MemberName = s.NameOfOfficer, CNIC = s.Cnic, s.BPSList.BPS, EventName = events.EventName, EventLocation = events.EventLocation, HeldDate = events.EntryDate, ProjectName = project.ProjectName, ProjectLocation = project.ProjectLocation, ProjectDescription = project.Description });

            var reportViewModel = new phaBalloting.Areas.Admin.Models.ReportViewModel()
            {
                FileName = "~/Content/reports/MemberReport.rdlc",
                //LeftMainTitle = "ABC Company Name",
                //LeftSubTitle = "DEF Department Name",
                //RightMainTitle = "اسم الشركة",
                //RightSubTitle = "اسم القسم",
                Name = "Statistical Report",
                //ReportDate = DateTime.Now,
                Logo = "~/Content/logo.jpg",
                ReportTitle = "Waiting List",
                SubTitle = " ",
                Contact = " ",
                ReportLanguage = "en-US",
                Format = phaBalloting.Areas.Admin.Models.ReportViewModel.ReportFormat.PDF,
                UserNamPrinting = "",
                ViewAsAttachment = false,
            };
            //adding the dataset information to the report view model object
            reportViewModel.ReportDataSets.Add(new phaBalloting.Areas.Admin.Models.ReportViewModel.ReportDataSet() { Data = data, DatasetName = "DataSet1" });
            return File(reportViewModel.RenderReport(), reportViewModel.LastmimeType);

        }
    }
}