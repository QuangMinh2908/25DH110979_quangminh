using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using _25DH110979_quangminh.Models;
using _25DH110979_quangminh.Models.ViewModel;

namespace _25DH110979_quangminh.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            if (Session["Username"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                string uname = model.Username?.Trim();
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                bool exists = db.User.Any(u => u.Username.ToLower() == uname.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã có người sử dụng.");
                    return View(model);
                }

                // Tạo User mới
                var user = new User
                {
                    Username = uname,
                    Password = model.Password.Trim(),
                    UserRole = "C"
                };
                db.User.Add(user);
                db.SaveChanges();

                // Tạo Customer mới
                var customer = new Customer
                {
                    CustomerName = model.FullName.Trim(),
                    CustomerEmail = model.Email?.Trim(),
                    CustomerPhone = model.Phone.Trim(),
                    CustomerAddress = model.Address?.Trim(),
                    Username = uname
                };
                db.Customer.Add(customer);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            if (Session["Username"] != null)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var inputUser = model.Username?.Trim();
                var inputPass = model.Password?.Trim();
                var user = db.User.FirstOrDefault(u => u.Username == inputUser && u.Password.Trim() == inputPass);
                if (user != null)
                {
                    // Lưu thông tin vào Session
                    Session["Username"] = user.Username.Trim();
                    Session["UserRole"] = user.UserRole.Trim();

                    // Lấy thông tin customer nếu là khách hàng
                    var customer = db.Customer.FirstOrDefault(c => c.Username == user.Username.Trim());
                    if (customer != null)
                    {
                        Session["CustomerName"] = customer.CustomerName;
                        Session["CustomerID"] = customer.CustomerID;
                    }
                    else
                    {
                        Session["CustomerName"] = user.Username.Trim();
                    }

                    // Nếu là Admin thì thiết lập AdminUser
                    if (user.UserRole.Trim() == "A")
                    {
                        Session["AdminUser"] = user.Username.Trim();
                    }

                    // Lưu cookie xác thực
                    FormsAuthentication.SetAuthCookie(user.Username.Trim(), model.RememberMe);

                    TempData["Success"] = "Đăng nhập thành công! Chào mừng " + (Session["CustomerName"] ?? user.Username);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // GET/POST: Account/Logout
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();

            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName) { Expires = DateTime.Now.AddDays(-1) };
                Response.Cookies.Add(cookie);
            }

            TempData["Success"] = "Đã đăng xuất tài khoản an toàn.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/ProfileInfo
        public ActionResult ProfileInfo()
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", new { returnUrl = Url.Action("ProfileInfo") });

            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                // Nếu là tài khoản Admin xem profile
                customer = new Customer
                {
                    CustomerName = username,
                    CustomerPhone = "0900000000",
                    CustomerEmail = "admin@mystore.com",
                    CustomerAddress = "Trụ sở chính",
                    Username = username
                };
            }

            var model = new ProfileVM
            {
                Username = username,
                FullName = customer.CustomerName,
                Email = customer.CustomerEmail,
                Phone = customer.CustomerPhone,
                Address = customer.CustomerAddress
            };
            return View(model);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var customer = db.Customer.FirstOrDefault(c => c.Username == username);
                if (customer != null)
                {
                    customer.CustomerName = model.FullName.Trim();
                    customer.CustomerEmail = model.Email?.Trim();
                    customer.CustomerPhone = model.Phone.Trim();
                    customer.CustomerAddress = model.Address?.Trim();
                    db.SaveChanges();

                    Session["CustomerName"] = customer.CustomerName;
                    TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
                }
                return RedirectToAction("ProfileInfo");
            }
            return View("ProfileInfo", model);
        }

        // GET: Account/ChangePassword
        public ActionResult ChangePassword()
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login");
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var oldPass = model.OldPassword?.Trim();
                var user = db.User.FirstOrDefault(u => u.Username == username && u.Password.Trim() == oldPass);
                if (user == null)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không chính xác.");
                    return View(model);
                }

                user.Password = model.NewPassword.Trim();
                db.SaveChanges();
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("ProfileInfo");
            }
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
