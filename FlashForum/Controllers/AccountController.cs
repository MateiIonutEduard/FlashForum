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
using FlashForum.Models;

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
                if(profile != null) return File(profile.image_content, profile.image_type);
            }

            var filename = Request.MapPath("~/Images/guest.jpg");
            byte[] data = System.IO.File.ReadAllBytes(filename);
            return File(data, "image/jpg");
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
            var host = new UserService();
            bool ok = await host.Login(email, password);
            var table = new Dictionary<string, dynamic>();

            if (ok)
            {
                var cookie = new HttpCookie("user", email);
                cookie.Expires = DateTime.Now.AddDays(1);
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
            var host = new UserService();
            var table = new Dictionary<string, dynamic>();
            bool ok = await host.Signup(username, password, email, level, file);

            if(!ok)
            {
                var cookie = new HttpCookie("user", email);
                cookie.Expires = DateTime.Now.AddDays(1);

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

        public ActionResult Recover()
        {
            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Request.Cookies.Remove("user");
            return RedirectToAction("Login", "Account");
        }
    }
}