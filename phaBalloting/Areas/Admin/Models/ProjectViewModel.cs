using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace phaBalloting.Areas.Admin.Models
{
    public class ProjectViewModel
    {

        public int Id { get; set; }

        public ProjectViewModel(int id)
        {
            this.Id = id;
            Data.phaEntities db = new Data.phaEntities();
            var project = db.Projects.Where(w => w.Id == id).FirstOrDefault();
            this.ProjectName = project.ProjectName;
            this.ProjectLocation = project.ProjectLocation;
            this.ProjectTypeId = project.ProjectTypeId;
            this.ProjectType = project.ProjectType.TypeName;
            this.TotalApplicableUnits = project.TotalApplicableUnits;
            this.Units = new List<ProjectUnitViewModel>();
            foreach (var unit in project.PojectUnits)
            {
                var unittoAdd = new ProjectUnitViewModel { Id = unit.Id, Description = unit.Description, UnitNumber = unit.UnitNumber };
                unittoAdd.Attributes = new List<AttributesViewModel>();
                foreach (var attribute in unit.ProjectUnitAttributes)
                {
                    unittoAdd.Attributes.Add(new AttributesViewModel { Id = attribute.Id, key = attribute.AttributesType.AttributeName, value = attribute.AttributeValue });
                }
                this.Units.Add(unittoAdd);
            }
        }
        public string ProjectName { get; set; }
        public string ProjectLocation { get; set; }
        public Nullable<int> ProjectTypeId { get; set; }
        public Nullable<int> TotalApplicableUnits { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public System.DateTime EntryDate { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }

        public string ProjectType { get; set; }
        public List<ProjectUnitViewModel> Units { get; set; }

    }
    public class ProjectUnitViewModel
    {
        public int Id { get; set; }
        public int UnitNumber { get; set; }
        public string Description { get; set; }

        public List<AttributesViewModel> Attributes { get; set; }


    }
    public class AttributesViewModel
    {
        public int Id { get; set; }
        public string key { get; set; }
        public string value { get; set; }



    }

    public class BallotingFinalResults
    {

        string q = string.Empty;
        public BallotingFinalResults(int eventId = 0, int projectId = 0, int memberId = 0, string customfilter = "",string datatype="sucess")
        {
            string filter = "Where dbo.Events.Id=" + eventId + "";
            if (eventId > 0)
            {
                filter = "Where dbo.Events.Id=" + eventId + "";
            }
            if (projectId > 0)
            {
                filter += " AND dbo.Projects.Id= " + projectId;
            }
            if (memberId > 0)
            {
                filter += " AND dbo.Members.Id= " + memberId;

            }
            if (!string.IsNullOrEmpty(customfilter))
            {
                filter = filter + " AND " + customfilter;
            }

            filter += datatype == "sucess" ? " AND dbo.Balloting.Id Not In(select BallotingId from dbo.CancelledBallotings)" : " AND dbo.Balloting.Id In(select BallotingId from dbo.CancelledBallotings)";
            if (!filter.StartsWith("Where"))
            {
                if (filter.StartsWith("AND"))
                {
                    filter = "WHERE " + filter.Remove(0, 3);
                }
                else
                {
                    filter = " WHERE " + filter;
                }
            }
            
            this.q = @"SELECT  p.BallotingId,  p.OldMFormNo, p.NameOfOfficer, p.Cnic, p.BPS, p.EventName,p.EventLocation, p.ProjectName, 
                     p.TypeName, p.ProjectLocation, p.TotalApplicableUnits,p.EntryDate, p.EventHeldDate, p.UnitNumber, 
                     p.Description,p.Floor,p.[Short Code], p.[Flat No],p.[Covered Area] 
from(SELECT     
                   dbo.Balloting.Id as BallotingId,   dbo.Members.OldMFormNo, dbo.Members.NameOfOfficer, dbo.Members.Cnic, dbo.BPSLists.BPS, dbo.Events.EventName, dbo.Events.EventLocation, dbo.Projects.ProjectName, 
                      dbo.ProjectTypes.TypeName, dbo.Projects.ProjectLocation, dbo.Projects.TotalApplicableUnits, dbo.Projects.EntryDate, dbo.Events.EntryDate AS EventHeldDate, dbo.PojectUnits.UnitNumber, 
                      dbo.PojectUnits.Description, dbo.AttributesTypes.AttributeName, dbo.ProjectUnitAttributes.AttributeValue
FROM dbo.Projects INNER JOIN
                      dbo.ProjectTypes ON dbo.Projects.ProjectTypeId = dbo.ProjectTypes.Id INNER JOIN
                      dbo.PojectUnits ON dbo.Projects.Id = dbo.PojectUnits.PojectId INNER JOIN
                      dbo.ProjectUnitAttributes ON dbo.PojectUnits.Id = dbo.ProjectUnitAttributes.UnitId INNER JOIN
                      dbo.AttributesTypes ON dbo.ProjectUnitAttributes.AttributeId = dbo.AttributesTypes.Id INNER JOIN
                      dbo.Balloting ON dbo.PojectUnits.Id = dbo.Balloting.ProjectUnitID INNER JOIN
                      dbo.Members ON dbo.Balloting.MemberID = dbo.Members.Id INNER JOIN
                      dbo.BPSLists ON dbo.Members.BPSId = dbo.BPSLists.Id INNER JOIN
                      dbo.Events ON dbo.Balloting.EventID = dbo.Events.Id " + filter + @"  ) as t
pivot(Max(AttributeValue) for AttributeName In([Floor],[Short Code], [Flat No],[Covered Area]) ) as p";

            //ORDER BY dbo.PojectUnits.UnitNumber FOR XML PATH('')
        }

        public System.Data.DataTable GetData(string sort = "")
        {
            DataTable data = new DataTable();

            Data.phaEntities db = new Data.phaEntities();
            return GetReportSummary(db.Database.Connection.ConnectionString, q);
        }

        public DataTable GetReportSummary(string connectionString, string query)
        {

            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.SelectCommand.CommandType = CommandType.Text;
                    adapter.Fill(ds);
                }
            }
            return ds.Tables[0];
        }
    }
}