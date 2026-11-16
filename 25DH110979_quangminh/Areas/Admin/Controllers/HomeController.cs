using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _25DH110979_quangminh.Models;

namespace _25DH110979_quangminh.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            ViewBag.TotalCategories = db.Category.Count();
            ViewBag.TotalProducts = db.Product.Count();
            ViewBag.TotalOrders = db.Order.Count();
            ViewBag.TotalCustomers = db.Customer.Count();
            ViewBag.TotalRevenue = db.Order.Where(o => o.PaymentStatus != "Đã hủy").Any() 
                ? db.Order.Where(o => o.PaymentStatus != "Đã hủy").Sum(o => (decimal?)o.TotalAmount) ?? 0 
                : 0;

            var recentOrders = db.Order.Include(o => o.Customer)
                                       .OrderByDescending(o => o.OrderID)
                                       .Take(5)
                                       .ToList();
            return View(recentOrders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
