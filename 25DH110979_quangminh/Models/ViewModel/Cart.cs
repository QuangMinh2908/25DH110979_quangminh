using System;
using System.Collections.Generic;
using System.Linq;

namespace _25DH110979_quangminh.Models.ViewModel
{
    [Serializable]
    public class Cart
    {
        private List<CartItem> _items = new List<CartItem>();

        public List<CartItem> Items => _items;

        public bool AddItem(Product product, int quantity = 1)
        {
            if (product == null) return false;

            var item = _items.FirstOrDefault(p => p.ProductID == product.ProductID);
            int availableStock = product.Quantity;

            if (item == null)
            {
                int qtyToAdd = Math.Min(quantity, availableStock > 0 ? availableStock : quantity);
                _items.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = string.IsNullOrEmpty(product.ProductImage) ? "default.png" : product.ProductImage,
                    UnitPrice = product.ProductPrice,
                    Quantity = qtyToAdd,
                    Stock = availableStock,
                    CategoryName = product.Category != null ? product.Category.CategoryName : "Khác",
                    IsSelected = true
                });
            }
            else
            {
                item.Stock = availableStock;
                item.Quantity += quantity;
                if (availableStock > 0 && item.Quantity > availableStock)
                {
                    item.Quantity = availableStock;
                }
            }
            return true;
        }

        public void RemoveItem(int productId)
        {
            _items.RemoveAll(p => p.ProductID == productId);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = _items.FirstOrDefault(p => p.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    if (item.Stock > 0 && quantity > item.Stock)
                    {
                        item.Quantity = item.Stock;
                    }
                    else
                    {
                        item.Quantity = quantity;
                    }
                }
                else
                {
                    _items.Remove(item);
                }
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        public decimal TotalAmount => _items.Where(i => i.IsSelected).Sum(i => i.TotalPrice);

        public int TotalItems => _items.Sum(i => i.Quantity);

        public int TotalQuantity => _items.Sum(i => i.Quantity);

        public IEnumerable<IGrouping<string, CartItem>> GroupedItems => _items.GroupBy(i => i.CategoryName);
    }
}
