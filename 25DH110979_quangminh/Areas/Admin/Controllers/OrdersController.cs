using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using _25DH110979_quangminh.Models;

namespace _25DH110979_quangminh.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders (Tìm theo Tên khách hàng, Trạng thái thanh toán, Ngày đặt hàng - Phân trang 15 đơn/trang)
        public ActionResult Index(string searchTerm, string status, DateTime? orderDate, int? page)
        {
            var query = db.Order.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                int orderId;
                if (int.TryParse(searchTerm, out orderId))
                {
                    query = query.Where(o => o.OrderID == orderId || o.Customer.CustomerName.Contains(searchTerm) || o.Customer.CustomerPhone.Contains(searchTerm));
                }
                else
                {
                    query = query.Where(o => o.Customer.CustomerName.Contains(searchTerm) || o.Customer.CustomerPhone.Contains(searchTerm));
                }
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(o => o.PaymentStatus == status);
            }

            if (orderDate.HasValue)
            {
                DateTime startDate = orderDate.Value.Date;
                DateTime endDate = startDate.AddDays(1);
                query = query.Where(o => o.OrderDate >= startDate && o.OrderDate < endDate);
            }

            int pageSize = 15;
            int pageNumber = page ?? 1;

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentStatus = status;
            ViewBag.OrderDate = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : "";

            return View(query.OrderByDescending(o => o.OrderID).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Orders/DetailedOrder/5
        public ActionResult DetailedOrder(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Order order = db.Order.Include(o => o.Customer)
                                 .Include(o => o.OrderDetail.Select(od => od.Product))
                                 .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            if (ViewEngines.Engines.FindView(ControllerContext, "DetailedOrder", null).View != null)
                return View("DetailedOrder", order);

            return View("Details", order);
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            return DetailedOrder(id);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int orderId, string status)
        {
            var order = db.Order.Include(o => o.OrderDetail.Select(od => od.Product)).FirstOrDefault(o => o.OrderID == orderId);
            if (order != null && !string.IsNullOrEmpty(status))
            {
                string oldStatus = order.PaymentStatus ?? "Chờ xử lý";

                if (oldStatus != "Đã hủy" && status == "Đã hủy")
                {
                    foreach (var detail in order.OrderDetail)
                    {
                        if (detail.Product != null)
                        {
                            detail.Product.Quantity += detail.Quantity;
                            detail.Product.SoldQuantity = Math.Max(0, detail.Product.SoldQuantity - detail.Quantity);
                        }
                    }
                }
                else if (oldStatus == "Đã hủy" && status != "Đã hủy")
                {
                    foreach (var detail in order.OrderDetail)
                    {
                        if (detail.Product != null)
                        {
                            detail.Product.Quantity = Math.Max(0, detail.Product.Quantity - detail.Quantity);
                            detail.Product.SoldQuantity += detail.Quantity;
                        }
                    }
                }

                order.PaymentStatus = status;
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng #{orderId} thành '{status}' thành công!";
            }
            return RedirectToAction("DetailedOrder", new { id = orderId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}