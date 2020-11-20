using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.Web.Http.Results;
using System.Data.Entity;

namespace FlashForum.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        public async Task<ActionResult> Show()
        {
            var db = new ForumEntity();
            var email = (Request.Cookies["user"] != null) ? Request.Cookies["user"].Value : "";
            var user = await db.Users.Where(u => u.user_email == email).FirstOrDefaultAsync();

            if (user != null)
            {
                var profile = await db.Profiles.Where(p => p.user_id == user.Id).FirstOrDefaultAsync();
                return File(profile.image_content, profile.image_type);
            }

            byte[] data = System.IO.File.ReadAllBytes(@"/Images/sys.png");
            return File(data, "image/png");
        }

        public async Task<ActionResult> ViewProfile()
        {
            var db = new ForumEntity();
            var cookie = Request.Cookies["user"];
            var table = new Dictionary<string, dynamic>();

            var user = await db.Users.Where(u => u.user_email == cookie.Value).FirstOrDefaultAsync();
            table.Add("username", user.user_name);
            table.Add("email", user.user_email);

            table.Add("admin", user.user_level);
            return Json(table, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Login(string email, string password)
        {
            var hash = SHA256.Create();
            var db = new ForumEntity();

            var table = new Dictionary<string, dynamic>();
            byte[] hashKey = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            string key = Convert.ToBase64String(hashKey);

            var user = await db.Users.Where(u => u.user_email == email && u.user_pass == key).FirstOrDefaultAsync();

            if (user != null)
            {
                var cookie = new HttpCookie("user", email);
                cookie.Expires.AddDays(1);
                HttpContext.Response.SetCookie(cookie);
                table.Add("url", "/Home/Index");

                table.Add("success", true);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
            else
            {
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Signup(string username, string password, string email, bool level, HttpPostedFileBase file)
        {
            var hash = SHA256.Create();
            var db = new ForumEntity();

            byte[] hashKey = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            string key = Convert.ToBase64String(hashKey);

            User node = new User
            {
                user_name = username,
                user_pass = key,
                user_email = email,
                user_date = DateTime.Now,
                user_level = level
            };

            db.Users.Add(node);
            await db.SaveChangesAsync();
            var table = new Dictionary<string, dynamic>();
            var RefUser = db.Users.Where(item => item.user_email == node.user_email && item.user_pass == key).FirstOrDefault();

            if(RefUser != null)
            {
                int userId = RefUser.Id;
                var ms = new MemoryStream();
                await file.InputStream.CopyToAsync(ms);

                var profile = new Profile
                {
                    image_name = Path.GetFileName(file.FileName),
                    image_type = file.ContentType,
                    image_content = ms.ToArray(),
                    user_id = RefUser.Id
                };

                db.Profiles.Add(profile);
                await db.SaveChangesAsync();

                var cookie = new HttpCookie("user", email);
                cookie.Expires.AddDays(1);

                HttpContext.Response.SetCookie(cookie);
                table.Add("action", "Register new user");

                table.Add("success", true);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
            else
            {
                table.Add("action", "User already exists!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Logout()
        {
            HttpContext.Request.Cookies.Remove("user");
            return RedirectToAction("Login", "Account");
        }
    }
}