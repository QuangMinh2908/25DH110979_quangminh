using PagedList;
using System.Collections.Generic;
using System.Web.Mvc;

namespace _25DH110979_quangminh.Models.ViewModel
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }
        public int? CategoryID { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortOrder { get; set; }
        public int? PageNumber { get; set; }
        public int PageSize { get; set; } = 8;

        public IPagedList<Product> Products { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}
