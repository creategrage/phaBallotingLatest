using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using phaBalloting.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace phaBalloting
{

    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class myAuthorizeAttribute : AuthorizeAttribute
    {

        //Custom named parameters for annotation
        public string ResourceKey { get; set; }
        public string OperationKey { get; set; }

        //Called when access is denied
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //User isn't logged in
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "Login", area = "" })
                );
            }
            //User is logged in but has no access
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "NotAuthorized", area = "" })
                );
            }
        }

        //Core authentication, called before each action
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var b = httpContext.User.Identity.IsAuthenticated;
            //Is user logged in?
            if (b)
                //If user is logged in and we need a custom check:
                if (ResourceKey != null && OperationKey != null)
                {
                    //httpContext.User.Foo("edit");
                    return false;
                }
            //Returns true or false, meaning allow or deny. False will call HandleUnauthorizedRequest above
            return b;
        }
    }
}