using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH110979_quangminh.Models;
using _25DH110979_quangminh.Models.ViewModel;

namespace _25DH110979_quangminh.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var query = db.Product.Include(p => p.Category).AsQueryable();

            // 1. Tìm kiếm theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p => p.ProductName.Contains(searchTerm) || p.ProductDecription.Contains(searchTerm));
            }

            // 2. Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            // 3. Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            // 4. Sắp xếp
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(p => p.ProductName);
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName);
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

            int pageSize = 8;
            int pageNumber = (page ?? 1);

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

            return View(model);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
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
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName");
            return View(new Product { ProductPrice = 0, ProductImage = "default.png", Quantity = 50, SoldQuantity = 0 });
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage,UploadImg,Quantity,SoldQuantity")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(product.UploadImg.FileName);
                    string extension = Path.GetExtension(product.UploadImg.FileName);
                    fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string uploadPath = Server.MapPath("~/Content/images/");
                    
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    product.UploadImg.SaveAs(Path.Combine(uploadPath, fileName));
                    product.ProductImage = fileName;
                }
                else if (string.IsNullOrEmpty(product.ProductImage))
                {
                    product.ProductImage = "default.png";
                }

                if (product.Quantity < 0) product.Quantity = 0;
                if (product.SoldQuantity < 0) product.SoldQuantity = 0;

                db.Product.Add(product);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm mới sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage,UploadImg,Quantity,SoldQuantity")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh mới nếu có
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(product.UploadImg.FileName);
                    string extension = Path.GetExtension(product.UploadImg.FileName);
                    fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string uploadPath = Server.MapPath("~/Content/images/");
                    
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    product.UploadImg.SaveAs(Path.Combine(uploadPath, fileName));
                    product.ProductImage = fileName;
                }

                if (product.Quantity < 0) product.Quantity = 0;
                if (product.SoldQuantity < 0) product.SoldQuantity = 0;

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
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
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            if (product != null)
            {
                if (product.OrderDetail.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì đã tồn tại trong các đơn hàng!";
                    return RedirectToAction("Index");
                }
                db.Product.Remove(product);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }
            return RedirectToAction("Index");
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
