using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace phaBalloting.Areas.Admin.Models
{
    public class ProjectViewModel
    {

        public int Id { get; set; }

        public ProjectViewModel(int id) {
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
                var unittoAdd = new ProjectUnitViewModel { Id=unit.Id, Description=unit.Description, UnitNumber=unit.UnitNumber };
                unittoAdd.Attributes = new List<AttributesViewModel>();
                foreach (var attribute in unit.ProjectUnitAttributes) {
                    unittoAdd.Attributes.Add(new AttributesViewModel { Id=attribute.Id, key=attribute.AttributesType.AttributeName, value=attribute.AttributeValue });
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
}