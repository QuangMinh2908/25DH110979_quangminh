using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PagedList;
using _25DH110979_quangminh.Models;

namespace _25DH110979_quangminh.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Users
        [AdminAuthorize]
        public ActionResult Index(string searchTerm, int? page)
        {
            var query = db.User.Where(u => u.UserRole == "A").AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(u => u.Username.Contains(searchTerm));
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            ViewBag.SearchTerm = searchTerm;

            return View(query.OrderBy(u => u.Username).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Users/Login
        public ActionResult Login(string returnUrl)
        {
            if (Session["AdminUser"] != null && Session["UserRole"] as string == "A")
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Admin/Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            username = username.Trim();
            password = password.Trim();

            var admin = db.User.FirstOrDefault(u => u.Username == username && u.Password.Trim() == password && u.UserRole == "A");
            if (admin != null)
            {
                Session["AdminUser"] = admin.Username;
                Session["Username"] = admin.Username;
                Session["UserRole"] = "A";

                FormsAuthentication.SetAuthCookie(admin.Username, false);

                TempData["SuccessMessage"] = "Đăng nhập quản trị viên thành công!";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác hoặc bạn không có quyền Quản trị!";
            return View();
        }

        // GET/POST: Admin/Users/Logout
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Logout()
        {
            Session.Remove("AdminUser");
            if (Session["UserRole"] as string == "A")
            {
                Session.Remove("UserRole");
                Session.Remove("Username");
                FormsAuthentication.SignOut();
            }
            TempData["SuccessMessage"] = "Đã đăng xuất khỏi tài khoản quản trị.";
            return RedirectToAction("Login");
        }

        // GET: Admin/Users/Create
        [AdminAuthorize]
        public ActionResult Create()
        {
            return View(new User { UserRole = "A" });
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult Create([Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                string uname = user.Username?.Trim();
                if (db.User.Any(u => u.Username.ToLower() == uname.ToLower()))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại trong hệ thống.");
                    return View(user);
                }

                user.Username = uname;
                user.Password = user.Password?.Trim();
                user.UserRole = "A"; // Luôn tạo với quyền Admin
                db.User.Add(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm mới tài khoản Admin thành công!";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Admin/Users/Edit/admin
        [AdminAuthorize]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Edit/admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult Edit([Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                user.UserRole = "A";
                user.Password = user.Password?.Trim();
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật tài khoản Admin thành công!";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Users/Delete/admin
        [AdminAuthorize]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Users/Delete/admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id.ToLower() == "admin")
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Admin mặc định tối cao!";
                return RedirectToAction("Index");
            }

            User user = db.User.Find(id);
            if (user != null)
            {
                db.User.Remove(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa tài khoản Admin thành công!";
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
