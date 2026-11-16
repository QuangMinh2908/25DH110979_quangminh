using System.Web.Mvc;

namespace _25DH110979_quangminh.Controllers
{
    // This controller redirects to Admin area - actual implementation is in Areas/Admin/Controllers/CategoriesController.cs
    public class CategoriesController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Categories", new { area = "Admin" });
        }
    }
}
