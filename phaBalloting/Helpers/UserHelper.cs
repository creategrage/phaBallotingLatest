using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using phaBalloting.Models;

namespace phaBalloting.Helpers
{
    public static class UserHelper
    {
       static Data.phaEntities db = new Data.phaEntities ();
        private static ApplicationSignInManager _signInManager;
        private static ApplicationUserManager _userManager;
        static  ApplicationDbContext context = new ApplicationDbContext();
        static RoleManager<IdentityRole> roleManager;
        public static ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public static ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public static int CurrentUserId {
            get { return 0; }// db.UserProfiles.ToList().Where(w => w.IdentityId == UserManager.FindByName(HttpContext.Current.User.Identity.Name).Id).FirstOrDefault().UserId; }
        }

        public static bool WriteActivity(string description) {

            var activity = new Data.UserActivity();
            activity.ClientDetail = "User accessed the page using IP: "+HttpContext.Current.Request.UserHostAddress+" Name of System: "+ System.Net.Dns.GetHostEntry(HttpContext.Current.Request.UserHostAddress).HostName + " Browser: " + ""+HttpContext.Current.Request.Browser.Platform+" version "+ HttpContext.Current.Request.Browser.Version ;
            activity.Description = description;
            activity.EntryDate = DateTime.Now;
            activity.UserId = HttpContext.Current.User.Identity.GetUserId();
            db.UserActivities.Add(activity);
            try {
                db.SaveChanges();
            } catch {
                return false;
            }

            return true;
        }
    }
}