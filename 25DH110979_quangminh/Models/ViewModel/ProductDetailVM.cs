using System.Collections.Generic;

namespace _25DH110979_quangminh.Models.ViewModel
{
    public class ProductDetailVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
        public List<Product> TopProducts { get; set; }
        public int QuantitySold { get; set; }
    }
}
