
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using phaBalloting.Data;
using phaBalloting.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using PagedList.Mvc;
using phaBalloting.Areas.Admin.Models;

namespace phaBalloting.Areas.Admin.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        //
        // GET: /Admin/Users/

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext context = new ApplicationDbContext();
        phaEntities db = new phaEntities();
        RoleManager<IdentityRole> roleManager;

        public UsersController()
        {
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        }

        public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index(int? pageNumber)
        {
            var model = UserManager.Users;
            var name = string.IsNullOrEmpty(Request["keywords"]) ? string.Empty : Request["keywords"].ToLower();
            if (name !=string.Empty)
            {
                return View(model.Where(x => x.UserName == name || name == null).ToList().ToPagedList(pageNumber ?? 1, 10));
            }
            return View(model.ToList().ToPagedList(1,10));
        }

        //public ActionResult Index()
        //{
            
        //    var model = UserManager.Users;
            //var viewModel = new List<UserViewModel>();
            //var sql = @"
            //SELECT AspNetUsers.UserName, AspNetRoles.Name As Role
            //FROM AspNetUsers 
            //LEFT JOIN AspNetUserRoles ON  AspNetUserRoles.UserId = AspNetUsers.Id 
            //LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId";
            ////WHERE AspNetUsers.Id = @Id";
            ////var idParam = new SqlParameter("Id", theUserId);

            //var result = context.Database.SqlQuery<UserViewModel>(sql).ToList();

        //    return View(model);
        //}

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {

                    var user = new ApplicationUser {  UserName = model.Email , Email = model.Email, PhoneNumber="", EmailConfirmed=true  };
                  var result=  UserManager.Create(user, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Users", new { area = "admin" });
                    }
                    else {
                        ModelState.AddModelError("", result.Errors.FirstOrDefault());
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }
            return View(model);
        }
        public ActionResult Detail(int id=0)
        {
            var model = UserManager.Users;
            if (model == null)
            {

            }
            return View(model);
        }

        public ActionResult Edit(string username)
        {
            string url = Request.QueryString["username"];
            var model = UserManager.FindByNameAsync(username);//  new Models.UsersContext().UserProfiles.Where(w => w.UserId == id).FirstOrDefault();

            if (model == null)
            {

            }
            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Models.UserProfile model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var db = new Models.UsersContext();
        //            db.Entry(model).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //        }
        //        catch (System.Data.Entity.Validation.DbEntityValidationException exception)
        //        {
        //            foreach (var inavlids in exception.EntityValidationErrors)
        //            {
        //                foreach (var inavlid in inavlids.ValidationErrors)
        //                {
        //                    ModelState.AddModelError(inavlid.PropertyName, inavlid.ErrorMessage);
        //                }
        //            }
        //        }
        //    }
        //    return View(model);
        //}
        public ActionResult Delete(string username)
        {
            string url = Request.QueryString["username"];
            var model = UserManager.FindByNameAsync(url);// new Models.UsersContext().UserProfiles.Where(w => w.UserId == id).FirstOrDefault();
            if (model == null)
            {

            } 
            return View(model);
        }
        public ActionResult UserDetails(string username)
        {
            string url = Request.QueryString["username"];
            return View();
        }
        
        

        public ActionResult RoleList()
        {
            var name = string.IsNullOrEmpty(Request["keywords"]) ? string.Empty : Request["keywords"].ToLower();
            if (string.IsNullOrEmpty(name))
            {
                ViewBag.Roles = roleManager.Roles.Select(s => s.Name).ToArray();
            }
            else {
                ViewBag.Roles = roleManager.Roles.Where(w=>w.Name==name ).Select(s => s.Name).ToArray();

            }
            return View();
        }


     
        public ActionResult CreateRole() {

            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateRole(string RoleName)
        {
             if (RoleName.ToLower() == "admin" || roleManager.RoleExists(RoleName))
            {
                ModelState.AddModelError("RoleName", "Provided role name already exist.");
            }
            if (ModelState.IsValid)
            {
                roleManager.Create(new IdentityRole { Name = RoleName });// Roles.CreateRole(RoleName);
                return RedirectToAction("RoleList");
            }
            ViewBag.rolename = RoleName;
            return View();
        }


        public ActionResult EditRole(string role)
        {
            ViewBag.rolename = role;
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditRole(string RoleName,bool post)
        {
            if (roleManager.RoleExists(RoleName))
            {
                var toedit =roleManager.FindByName(RoleName);
                roleManager.Update(toedit);
            }

            return RedirectToAction("RoleList");
        }

        public ActionResult AssignRole(string role="")
        {
            
            ViewBag.roles =new SelectList( roleManager.Roles,"Name","Name",role);
            ViewBag.users = new SelectList(UserManager.Users, "Id", "UserName");//new SelectList( Membership.GetAllUsers());

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRole(string userId, string rolename)
        {
            try{
                UserManager.AddToRole(userId, rolename);
                return RedirectToAction("UsersInRole", new { role = rolename });
        }catch{}

            ViewBag.roles = new SelectList(roleManager.Roles,"Name","Name",rolename);
            ViewBag.users = new SelectList(UserManager.Users, "Id", "UserName", userId); //new SelectList( Membership.GetAllUsers());
            return View();
        }

        public ActionResult UsersInRole(string role)
        {
            ViewBag.rolename = role;
            ViewBag.users = UserManager.Users.ToList().Where(w => UserManager.IsInRole(w.Id, role)).Select(s => s.UserName).ToArray(); 
            return View();
        }

        public ActionResult RemoveUserFromRole(string username, string rolename)
        {
            try {
                string userId = UserManager.FindByName(username).Id;
                UserManager.RemoveFromRole(userId, rolename);
            }
            catch { }

            return RedirectToAction("UsersInRole", new { role=rolename});
        }
        
        public ActionResult Authentications(string role)
        {
            string roleid = db.AspNetRoles.Where(w => w.Name == role).FirstOrDefault().Id;
            return View(AuthenticationModel.GetAll(roleid));
        }

        [HttpPost]
        public JsonResult SaveOrUpdate(Authentication auth)
        {
            string message = "false";

            if (AuthenticationModel.SaveOrUpdate(auth))
            {
                message = "true";
            }

            return new JsonResult { Data = message };
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}
