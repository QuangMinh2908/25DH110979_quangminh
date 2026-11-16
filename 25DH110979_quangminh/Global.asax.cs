using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace _25DH110979_quangminh
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            try
            {
                var context = HttpContext.Current;
                if (context != null && context.Session != null && context.User != null && context.User.Identity.IsAuthenticated)
                {
                    if (context.Session["Username"] == null)
                    {
                        string username = context.User.Identity.Name;
                        using (var db = new Models.MyStoreEntities())
                        {
                            var user = db.User.FirstOrDefault(u => u.Username == username);
                            if (user != null)
                            {
                                context.Session["Username"] = user.Username.Trim();
                                context.Session["UserRole"] = user.UserRole.Trim();
                                if (user.UserRole.Trim() == "A")
                                {
                                    context.Session["AdminUser"] = user.Username.Trim();
                                }
                                var customer = db.Customer.FirstOrDefault(c => c.Username == username);
                                if (customer != null)
                                {
                                    context.Session["CustomerName"] = customer.CustomerName;
                                    context.Session["CustomerID"] = customer.CustomerID;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore during unmanaged requests
            }
        }
    }
}
