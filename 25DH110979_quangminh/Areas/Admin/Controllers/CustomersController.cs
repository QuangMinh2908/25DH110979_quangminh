using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using _25DH110979_quangminh.Models;

namespace _25DH110979_quangminh.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            var query = db.Customer.Include(c => c.Order).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(c => c.CustomerName.Contains(searchTerm) || 
                                         c.CustomerPhone.Contains(searchTerm) || 
                                         c.CustomerEmail.Contains(searchTerm) ||
                                         c.Username.Contains(searchTerm));
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SearchTerm = searchTerm;

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(c => c.CustomerName);
                    break;
                case "name_asc":
                    query = query.OrderBy(c => c.CustomerName);
                    break;
                default:
                    query = query.OrderByDescending(c => c.CustomerID);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(query.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Include(c => c.Order).FirstOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
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
