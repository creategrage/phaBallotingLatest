using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using phaBalloting.Data;

namespace phaBalloting.Areas.Admin.Models
{
    public static class AuthenticationModel 
    {
        private static phaEntities database = new phaEntities();

        public static bool Add(Authentication entity)
        {
            database.Authentications.Add(entity);
            try
            {
                database.SaveChanges();
                return true;
            }
            catch (Exception aa){ return false; }
        }

        public static bool Edit(Authentication entity)
        {
            Authentication auth = database.Authentications.Where(w => w.Id == entity.Id).FirstOrDefault();
            auth.ViewRecords = entity.ViewRecords;
            auth.EditRecords = entity.EditRecords;
            auth.DeleteRecords = entity.DeleteRecords;
            auth.AddRecords = entity.AddRecords;
            try
            {
                database.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public static bool SaveOrUpdate(Authentication entity)
        {
            bool success = false;
            if (entity.Id < 1)
            {
                success = Add(entity);
            }
            else
            {
                success = Edit(entity);
            }
            return success;
        }

        public class authenticationClass
        {
            public int ID { get; set; }
            public string RoleId { get; set; }
            public int FormID { get; set; }
            public string FormName { get; set; }
            public string Description { get; set; }
            public bool ViewRecords { get; set; }
            public bool AddRecords { get; set; }
            public bool EditRecords { get; set; }
            public bool DeleteRecords { get; set; }
        }

        public static List<authenticationClass> GetAll(string RoleId)
        {
            var list = database.AppForms.Select(s => new authenticationClass
            {
                RoleId = RoleId,
                FormID = s.Id,
                ID = s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault() == null ? -1 : s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault().Id,
                FormName = s.FormName,
                Description = s.Description,
                AddRecords = s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault().AddRecords.Value,
                EditRecords = s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault().EditRecords.Value,
                DeleteRecords = s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault().DeleteRecords.Value,
                ViewRecords = s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleId == RoleId).FirstOrDefault().ViewRecords.Value
            });
            return list.ToList();
        }

        

        public static Authentication GetByID(int id)
        {
            return database.Authentications.Where(w => w.Id == id).FirstOrDefault();
        }

        
    }
}