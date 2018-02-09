using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using phaBalloting.Models;
using System.Data.SqlClient;

namespace phaBalloting.Helpers
{
    public static class DBMaintanance
    {
       static Data.phaEntities db = new Data.phaEntities ();

        public static bool Backup(string path)
        {

            string query = @"BACKUP DATABASE  " + db.Database.Connection.Database + " TO DISK = '" + path + "'";
            try
            {
                using (SqlConnection connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                //db.Database.ExecuteSqlCommand(q, null);
                return true;
            }
            catch(Exception aa) {
                string a = aa.Message;
            }
            return false;
        }
        public static bool Restore(string path)
        {

            string query = @"Use[master]  RESTORE  DATABASE " + db.Database.Connection.Database + " FROM DISK = '" + path + "'";
            try
            {
                using (SqlConnection connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                //db.Database.ExecuteSqlCommand(q, null);
                return true;
            }
            catch(Exception aa)
            {
                return false;
            }
        }
    }
}