using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using _25DH110979_quangminh.Models;
using _25DH110979_quangminh.Models.ViewModel;

namespace _25DH110979_quangminh.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? categoryId, int? page)
        {
            var query = db.Product.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p => p.ProductName.Contains(searchTerm) || p.ProductDecription.Contains(searchTerm));
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            // Top 10 sản phẩm nổi bật (sắp xếp theo số lượng đã bán SoldQuantity hoặc OrderDetail.Sum)
            var featured = db.Product.Include(p => p.Category)
                                     .OrderByDescending(p => p.SoldQuantity)
                                     .ThenByDescending(p => p.ProductID)
                                     .Take(10)
                                     .ToList();

            // 20 sản phẩm mới nhất, 6 sản phẩm/trang
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var newProductsList = query.OrderByDescending(p => p.ProductID)
                                       .Take(20)
                                       .ToPagedList(pageNumber, pageSize);

            var model = new HomeProductVM
            {
                SearchTerm = searchTerm,
                CategoryID = categoryId,
                FeaturedProducts = featured,
                NewProducts = newProductsList,
                Categories = db.Category.ToList()
            };

            ViewBag.CurrentCategory = categoryId;
            return View(model);
        }

        // GET: Home/ProductList
        public ActionResult ProductList(string searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var query = db.Product.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p => p.ProductName.Contains(searchTerm) || p.ProductDecription.Contains(searchTerm));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
                var cat = db.Category.Find(categoryId.Value);
                ViewBag.CategoryName = cat != null ? cat.CategoryName : "Tất cả sản phẩm";
            }
            else
            {
                ViewBag.CategoryName = "Tất cả sản phẩm";
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            ViewBag.CurrentSort = sortOrder;
            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    query = query.OrderByDescending(p => p.ProductID);
                    break;
            }

            int pageSize = 9;
            int pageNumber = page ?? 1;

            var model = new ProductSearchVM
            {
                SearchTerm = searchTerm,
                CategoryID = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Products = query.ToPagedList(pageNumber, pageSize),
                CategoryList = new SelectList(db.Category, "CategoryID", "CategoryName", categoryId)
            };

            ViewBag.CurrentCategory = categoryId;
            ViewBag.Categories = db.Category.ToList();

            return View(model);
        }

        // GET: Home/ProductDetail/5
        public ActionResult ProductDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Product.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // 8 sản phẩm liên quan cùng danh mục
            var related = db.Product.Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                                    .Take(8)
                                    .ToList();

            // 8 sản phẩm bán chạy cùng danh mục
            var topProducts = db.Product.Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                                        .OrderByDescending(p => p.SoldQuantity)
                                        .Take(8)
                                        .ToList();

            int quantitySold = product.SoldQuantity > 0 ? product.SoldQuantity : (product.OrderDetail.Sum(od => (int?)od.Quantity) ?? 0);

            var model = new ProductDetailVM
            {
                Product = product,
                RelatedProducts = related,
                TopProducts = topProducts,
                QuantitySold = quantitySold
            };

            ViewBag.CurrentCategory = product.CategoryID;
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult GetCategories()
        {
            var categories = db.Category.ToList();
            return PartialView("_CategoriesPV", categories);
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
