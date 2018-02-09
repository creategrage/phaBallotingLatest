using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Data.SqlClient;

namespace phaBalloting.Helpers
{
    public static class Extensions
    {
        //public static Microsoft.AspNet.Identity.IRole RoleAuthenticated(this role)
        //{

        //}
       public static Data.phaEntities db = new Data.phaEntities();
       

        public static string[] GetRoles(this string username)
        {
            string role = string.Empty;
            var sql = @"
            SELECT AspNetRoles.Name As Role
            FROM AspNetUsers 
            LEFT JOIN AspNetUserRoles ON  AspNetUserRoles.UserId = AspNetUsers.Id 
            LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId WHERE AspNetUsers.UserName = @UserName";
            var idParam = new SqlParameter("UserName", username);

            var result = db.Database.SqlQuery<Models.UserViewModel>(sql, idParam).ToList();

            return result.Select(s=>s.Role).ToArray();
        }

        public static  bool IsInRoleNew(this IPrincipal principal, string Role)
        {
            return principal.Identity.Name.GetRoles().Contains("Admin");
        }
        public static string[] IsRoleAuthrozed(this string username)
        {
            string role = string.Empty;
            var sql = @"SELECT AspNetRoles.Name As Role FROM AspNetUsers 
            LEFT JOIN AspNetUserRoles ON  AspNetUserRoles.UserId = AspNetUsers.Id 
            LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId WHERE AspNetUsers.UserName = @UserName";
            var idParam = new SqlParameter("UserName", username);
            var result = db.Database.SqlQuery<Models.UserViewModel>(sql, idParam).ToList();
            return result.Select(s => s.Role).ToArray();
        }

        public static bool IsAuthrozed(this EnumManager.Modules module, EnumManager.Actions action)
        {
            if (HttpContext.Current.User.IsInRoleNew("Admin"))
            {
                return true;
            }
            string sql = string.Empty;
            if (action == EnumManager.Actions.Any)
            {
                sql = @"SELECT Authentications.* from Authentications, AspNetRoles Where AspNetRoles.Name In(" + string.Join(",", HttpContext.Current.User.Identity.Name.GetRoles()) + ") and (Authentications.ViewRecords=1 OR Authentications.EditRecords=1 OR Authentications.DeleteRecords=1 OR Authentications.AddRecords=1)  and FormId=" + module.GetId();

            }
            else sql = @"SELECT Authentications." + action.ToString() + " from Authentications, AspNetRoles Where AspNetRoles.Name In(" + string.Join(",", HttpContext.Current.User.Identity.Name.GetRoles()) + ") and Authentications." + action.ToString() + "=1 and FormId=" + module.GetId();
           
                var result = db.Database.SqlQuery<Areas.Admin.Models.AuthenticationModel.authenticationClass>(sql).ToList();
                if (result.Any())
                {
                    return true;
                }
            

            
            return false;
        }

        public static int GetId(this EnumManager.Modules module) {
            int id = 0;
            var modules = db.AppForms.ToList().Where(w => w.FormName.ToLower() == module.ToString().ToLower()).FirstOrDefault();
            if (modules != null) {
                id = modules.Id;
            }
            return id;
        }

    }
}