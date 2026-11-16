using System;
using System.Web;
using System.Web.Mvc;
using _25DH110979_quangminh.Models;
using _25DH110979_quangminh.Models.ViewModel;

namespace _25DH110979_quangminh.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(this.Session);
        }

        // GET: Cart
        public ActionResult Index()
        {
            var cart = GetCartService().GetCart();
            return View(cart);
        }

        // GET/POST: Cart/AddToCart
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult AddToCart(int productId, int qty = 1)
        {
            var product = db.Product.Find(productId);
            if (product == null)
                return HttpNotFound();

            if (product.Quantity <= 0)
            {
                TempData["Warning"] = $"Sản phẩm '{product.ProductName}' hiện đang tạm hết hàng!";
                return RedirectToAction("Index");
            }

            var cartService = GetCartService();
            var cart = cartService.GetCart();
            var existingItem = cart.Items.Find(i => i.ProductID == productId);
            int currentInCart = existingItem != null ? existingItem.Quantity : 0;

            if (currentInCart + qty > product.Quantity)
            {
                int maxCanAdd = product.Quantity - currentInCart;
                if (maxCanAdd > 0)
                {
                    cartService.AddToCart(product, maxCanAdd);
                    TempData["Warning"] = $"Chỉ có thể thêm tối đa {maxCanAdd} sản phẩm '{product.ProductName}' do giới hạn tồn kho ({product.Quantity}).";
                }
                else
                {
                    TempData["Warning"] = $"Bạn đã có {currentInCart} sản phẩm '{product.ProductName}' trong giỏ (bằng tồn kho hiện có)!";
                }
            }
            else
            {
                cartService.AddToCart(product, qty);
                TempData["Success"] = $"Đã thêm {qty} sản phẩm '{product.ProductName}' vào giỏ hàng!";
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, totalItems = cart.TotalItems }, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index");
        }

        // GET/POST: Cart/RemoveFromCart
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult RemoveFromCart(int productId)
        {
            GetCartService().RemoveFromCart(productId);
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        // GET/POST: Cart/ClearCart
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            TempData["Success"] = "Đã làm trống giỏ hàng.";
            return RedirectToAction("Index");
        }

        // GET/POST: Cart/UpdateQuantity
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var product = db.Product.Find(productId);
            if (product != null && quantity > product.Quantity)
            {
                GetCartService().UpdateQuantity(productId, product.Quantity);
                TempData["Warning"] = $"Số lượng sản phẩm '{product.ProductName}' được điều chỉnh về tối đa tồn kho ({product.Quantity}).";
            }
            else
            {
                GetCartService().UpdateQuantity(productId, quantity);
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
