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

        // GET: Admin/Orders
        public ActionResult Index(string searchTerm, string status, int? page)
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

            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentStatus = status;

            return View(query.OrderByDescending(o => o.OrderID).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Order.Include(o => o.Customer)
                                 .Include(o => o.OrderDetail.Select(od => od.Product))
                                 .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
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

                // Xử lý tăng giảm số lượng tồn kho khi hủy đơn hoặc khôi phục đơn
                if (oldStatus != "Đã hủy" && status == "Đã hủy")
                {
                    // Hoàn trả số lượng tồn kho khi hủy đơn
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
                    // Trừ lại số lượng tồn kho nếu kích hoạt lại đơn đã hủy
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
            return RedirectToAction("Details", new { id = orderId });
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
