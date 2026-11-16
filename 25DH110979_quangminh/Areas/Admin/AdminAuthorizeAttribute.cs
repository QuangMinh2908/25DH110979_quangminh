using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace _25DH110979_quangminh.Areas.Admin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;

            var session = httpContext.Session;
            if (session != null)
            {
                var userRole = session["UserRole"] as string;
                var adminUser = session["AdminUser"] as string;
                if (!string.IsNullOrEmpty(adminUser) && userRole == "A")
                {
                    return true;
                }
            }

            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                string username = httpContext.User.Identity.Name;
                using (var db = new Models.MyStoreEntities())
                {
                    var user = db.User.Find(username);
                    if (user != null && user.UserRole.Trim() == "A")
                    {
                        if (session != null)
                        {
                            session["AdminUser"] = user.Username.Trim();
                            session["Username"] = user.Username.Trim();
                            session["UserRole"] = "A";
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "area", "Admin" },
                    { "controller", "Users" },
                    { "action", "Login" },
                    { "returnUrl", filterContext.HttpContext.Request.RawUrl }
                });
        }
    }
}
