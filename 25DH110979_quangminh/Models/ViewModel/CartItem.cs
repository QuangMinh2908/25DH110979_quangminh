using System;

namespace _25DH110979_quangminh.Models.ViewModel
{
    [Serializable]
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; } = 50;
        public string CategoryName { get; set; }
        public bool IsSelected { get; set; } = true;

        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
