using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _25DH110979_quangminh.Models;
using _25DH110979_quangminh.Models.ViewModel;

namespace _25DH110979_quangminh.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });

            var cart = GetCartService().GetCart();
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            var model = new CheckoutVM
            {
                FullName = customer?.CustomerName,
                Phone = customer?.CustomerPhone,
                ShippingAddress = customer?.CustomerAddress,
                Email = customer?.CustomerEmail,
                Cart = cart
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var cart = GetCartService().GetCart();
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["Warning"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            model.Cart = cart;

            if (ModelState.IsValid)
            {
                var customer = db.Customer.FirstOrDefault(c => c.Username == username);
                if (customer == null)
                    return RedirectToAction("Login", "Account");

                foreach (var item in cart.Items)
                {
                    var prod = db.Product.Find(item.ProductID);
                    if (prod == null || prod.Quantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' không đủ số lượng trong kho (chỉ còn {prod?.Quantity ?? 0} sản phẩm). Vui lòng cập nhật giỏ hàng!");
                        return View(model);
                    }
                }

                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalAmount,
                    ShippingAddress = model.ShippingAddress.Trim(),
                    DeliveryMethod = string.IsNullOrEmpty(model.DeliveryMethod) ? "Giao hàng thường" : model.DeliveryMethod,
                    PaymentMethod = string.IsNullOrEmpty(model.PaymentMethod) ? "Tiền mặt khi nhận hàng" : model.PaymentMethod,
                    PaymentStatus = "Chờ xử lý"
                };
                db.Order.Add(order);
                db.SaveChanges();

                foreach (var item in cart.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    };
                    db.OrderDetail.Add(detail);

                    var prod = db.Product.Find(item.ProductID);
                    if (prod != null)
                    {
                        prod.Quantity -= item.Quantity;
                        prod.SoldQuantity += item.Quantity;
                    }
                }
                db.SaveChanges();

                GetCartService().ClearCart();

                TempData["Success"] = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
                return RedirectToAction("OrderSuccess", new { orderId = order.OrderID });
            }

            return View(model);
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int orderId)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var order = db.Order.Find(orderId);
            if (order == null)
                return HttpNotFound();

            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            if (order.CustomerID != customer?.CustomerID && Session["UserRole"] as string != "A")
                return HttpNotFound();

            var details = db.OrderDetail.Include(od => od.Product).Where(od => od.OrderID == orderId).ToList();
            ViewBag.OrderDetails = details;
            return View(order);
        }

        // GET: Order/MyOrder
        public ActionResult MyOrder(string searchTerm, string status)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var query = db.Order.Include(o => o.OrderDetail.Select(od => od.Product))
                                .Where(o => o.CustomerID == customer.CustomerID)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                int orderId;
                if (int.TryParse(searchTerm, out orderId))
                {
                    query = query.Where(o => o.OrderID == orderId || o.OrderDetail.Any(od => od.Product.ProductName.Contains(searchTerm)));
                }
                else
                {
                    query = query.Where(o => o.OrderDetail.Any(od => od.Product.ProductName.Contains(searchTerm)));
                }
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả đơn" && status != "Tất cả")
            {
                if (status == "Chờ thanh toán")
                {
                    query = query.Where(o => o.PaymentStatus == "Chờ thanh toán" || o.PaymentStatus == "Chờ xử lý");
                }
                else
                {
                    query = query.Where(o => o.PaymentStatus == status);
                }
            }

            ViewBag.CurrentStatus = status;
            ViewBag.SearchTerm = searchTerm;

            var orders = query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID).Take(50).ToList();
            return View(orders);
        }

        // GET: Order/BuyAgainItem?productId=5
        public ActionResult BuyAgainItem(int productId)
        {
            var product = db.Product.Find(productId);
            if (product != null)
            {
                GetCartService().AddToCart(product, 1);
                TempData["Success"] = $"Đã thêm '{product.ProductName}' (số lượng: 1) vào giỏ hàng!";
                return RedirectToAction("Index", "Cart");
            }
            return RedirectToAction("MyOrder");
        }

        // POST/GET: Order/CancelOrder/5
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult CancelOrder(int id)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var order = db.Order.Include(o => o.OrderDetail.Select(od => od.Product))
                                .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);

            if (order == null)
            {
                TempData["Warning"] = "Không tìm thấy đơn hàng cần hủy.";
                return RedirectToAction("MyOrder");
            }

            if (order.PaymentStatus != "Chờ xử lý" && order.PaymentStatus != "Chờ thanh toán")
            {
                TempData["Warning"] = $"Không thể hủy đơn hàng #{id} vì đơn đang ở trạng thái '{order.PaymentStatus}'.";
                return RedirectToAction("MyOrder");
            }

            foreach (var detail in order.OrderDetail)
            {
                if (detail.Product != null)
                {
                    detail.Product.Quantity += detail.Quantity;
                    detail.Product.SoldQuantity = Math.Max(0, detail.Product.SoldQuantity - detail.Quantity);
                }
            }

            order.PaymentStatus = "Đã hủy";
            db.SaveChanges();

            TempData["Success"] = $"Đã hủy đơn hàng #{id} thành công! Số lượng hàng đã được hoàn lại vào kho.";
            return RedirectToAction("MyOrder");
        }

        // GET: Order/ReOrder/5
        public ActionResult ReOrder(int id)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var order = db.Order.Include(o => o.OrderDetail.Select(od => od.Product))
                                .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order != null)
            {
                var cartService = GetCartService();
                foreach (var item in order.OrderDetail)
                {
                    if (item.Product != null)
                        cartService.AddToCart(item.Product, 1);
                    else
                        cartService.AddToCart(item.ProductID, 1);
                }
                TempData["Success"] = $"Đã thêm các sản phẩm từ đơn hàng #{id} (số lượng = 1) vào giỏ hàng!";
                return RedirectToAction("Index", "Cart");
            }

            return RedirectToAction("MyOrder");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}