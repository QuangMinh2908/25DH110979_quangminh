using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace _25DH110979_quangminh.Models.ViewModel
{
    public class CartService
    {
        private const string CartSessionKey = "CartSessionKey";
        private HttpSessionStateBase _session;

        public CartService(HttpSessionStateBase session)
        {
            _session = session;
        }

        public static Cart GetCart(HttpSessionStateBase session)
        {
            if (session == null) return new Cart();
            var cart = session[CartSessionKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session[CartSessionKey] = cart;
            }
            return cart;
        }

        public static Cart GetCart(HttpSessionState session)
        {
            if (session == null) return new Cart();
            var cart = session[CartSessionKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session[CartSessionKey] = cart;
            }
            return cart;
        }

        public Cart GetCart()
        {
            return GetCart(_session);
        }

        public void AddToCart(Product product, int quantity = 1)
        {
            var cart = GetCart();
            cart.AddItem(product, quantity);
            if (_session != null)
            {
                _session[CartSessionKey] = cart;
            }
        }

        public void AddToCart(int productId, int quantity = 1)
        {
            using (var db = new MyStoreEntities())
            {
                var product = db.Product.Include(p => p.Category).FirstOrDefault(p => p.ProductID == productId);
                if (product != null)
                {
                    AddToCart(product, quantity);
                }
            }
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            if (_session != null)
            {
                _session[CartSessionKey] = cart;
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            if (_session != null)
            {
                _session[CartSessionKey] = cart;
            }
        }

        public void ClearCart()
        {
            if (_session != null)
            {
                _session.Remove(CartSessionKey);
            }
        }
    }
}
