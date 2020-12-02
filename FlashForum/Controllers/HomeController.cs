using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FlashForum.Models;
using System.Net;

namespace FlashForum.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddCategory(string name, string content)
        {
            var db = new ForumEntity();
            var table = new Dictionary<string, dynamic>();

            var host = new CategoryService();
            var success = await host.CreateCategory(name, content);

            if (!success)
            {
                table.Add("message", "Category was created once!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }

            table.Add("message", "Category has been created successfully!");
            table.Add("success", true);
            return Json(table, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveCategory(int? id)
        {
            var db = new ForumEntity();
            var table = new Dictionary<string, dynamic>();

            var email = Request.Cookies["user"] != null ? Request.Cookies["user"].Value : "";
            var user = await db.Users.Where(node => node.user_email == email).FirstOrDefaultAsync();

            if(user == null || user?.user_level == false)
            {
                table.Add("message", "Unauthorized");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }

            if(id == null)
            {
                table.Add("message", "No category specified!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }

            var host = new CategoryService();
            var status = await host.DeleteCategory(id);

            if(status == 1)
            {
                // return success
                table.Add("message", "The category of topics removed successfully!");
                table.Add("success", true);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // return failure
                table.Add("message", "Category does not exists!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AllTopics()
        {
            return View();
        }

        public ActionResult AddTopic()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddTopic(int id, string subject, string question, HttpPostedFileBase view)
        {
            var db = new ForumEntity();
            var email = Request.Cookies["user"] != null ? Request.Cookies["user"].Value : "";
            var user = await db.Users.Where(node => node.user_email == email).FirstOrDefaultAsync();
            var type = await db.Categories.Where(node => node.cat_id == id).FirstOrDefaultAsync();

            bool user_exists = (user != null);
            bool cat_exists = (type != null);

            var table = new Dictionary<string, dynamic>();
            bool ok = user_exists && cat_exists;

            if (ok)
            {
                var topic = new Topic
                {
                    topic_cat = id,
                    topic_subject = subject,
                    topic_date = DateTime.Now,
                    topic_by = user.Id,
                    status = 1
                };

                var QueryTopic = await db.Topics.Where(node => node.topic_subject == subject).FirstOrDefaultAsync();
                bool exists = (QueryTopic != null) ? true : false;

                if(!exists)
                {
                    db.Topics.Add(topic);
                    await db.SaveChangesAsync();
                    var top = await db.Topics.Where(t => t.topic_subject == subject).FirstOrDefaultAsync();

                    var post = new Post
                    {
                        post_content = question,
                        post_date = DateTime.Now,
                        post_topic = top.topic_id,
                        post_by = user.Id
                    };

                    db.Posts.Add(post);
                    await db.SaveChangesAsync();
                    var ms = new MemoryStream();

                    if (view != null)
                    {
                        await view.InputStream.CopyToAsync(ms);
                        var item = await db.Posts.Where(p => p.post_content == question).FirstOrDefaultAsync();

                        var file = new PostFile
                        {
                            file_name = Path.GetFileName(view.FileName),
                            file_type = view.ContentType,
                            file_content = ms.ToArray(),
                            ref_id = item.post_id
                        };

                        db.PostFiles.Add(file);
                        await db.SaveChangesAsync();
                    }

                    table.Add("message", "New topic has been successfully registered!");
                    table.Add("success", true);
                    return Json(table, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    table.Add("message", "This topic already exists!");
                    table.Add("success", false);
                    return Json(table, JsonRequestBehavior.AllowGet);
                }
            } 
            else
            {
                table.Add("message", "Something went wrong!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPut]
        public async Task<ActionResult> CloseTopic(int? id)
        {
            if(id != null)
            {
                var db = new ForumEntity();
                var topic = await db.Topics.Where(t => t.topic_id == id.Value)
                        .FirstOrDefaultAsync();

                topic.status = 3;
                await db.SaveChangesAsync();
                var result = new HttpStatusCodeResult(HttpStatusCode.OK);
                return result;
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Something is wrong!");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveTopic(int? id)
        {
            var db = new ForumEntity();
            var table = new Dictionary<string, dynamic>();

            if (id != null)
            {
                var email = (Request.Cookies["user"] != null) ? Request.Cookies["user"].Value : "";
                var user = await db.Users.Where(node => node.user_email == email).FirstOrDefaultAsync();
                
                if(user != null)
                {
                    var item = await db.Topics.Where(node => node.topic_id == id.Value && (node.topic_by == user.Id || user.user_level))
                        .FirstOrDefaultAsync();

                    if (item != null)
                    {
                        // remove topic successfully...
                        var posts = await db.Posts.Where(node => node.post_topic == item.topic_id).ToListAsync();

                        posts.ForEach((post) =>
                        {
                            var files = db.PostFiles.Where(node => node.ref_id == post.post_id).ToList();
                            if (files.Count != 0) db.PostFiles.RemoveRange(files);
                        });

                        if(posts.Count != 0) db.Posts.RemoveRange(posts);
                        db.Topics.Remove(item);
                        await db.SaveChangesAsync();

                        table.Add("message", "All posts and topic itself have been removed!");
                        table.Add("success", true);
                        return Json(table, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        table.Add("message", "There is not exists this topic!");
                        table.Add("success", false);
                        return Json(table, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    table.Add("message", "You are not authorized to remove data!");
                    table.Add("success", false);
                    return Json(table, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                table.Add("message", "Must to specify the topic in order to remove him!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Posts()
        {
            return View();
        }

        public async Task<ActionResult> ViewProfile(int id)
        {
            var db = new ForumEntity();
            var profile = await db.Profiles.Where(node => node.user_id == id).FirstOrDefaultAsync();

            if (profile != null) return File(profile.image_content, profile.image_type, profile.image_name);
            else
            {
                var filename = Request.MapPath("~/Images/guest.jpg");
                byte[] data = System.IO.File.ReadAllBytes(filename);
                return File(data, "image/jpg");
            }
        }

        public async Task<ActionResult> DownloadFile(int? id)
        {
            if (Request.Cookies["user"] != null && id != null)
            {
                var db = new ForumEntity();
                var file = await db.PostFiles.Where(node => node.Id == id.Value).FirstOrDefaultAsync();
                return File(file.file_content, file.file_type, file.file_name);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Something is wrong!");
        }

        [HttpPost]
        public async Task<ActionResult> SendPost(int topic, string message, HttpPostedFileBase file)
        {
            if(Request.Cookies["user"] != null)
            {
                var db = new ForumEntity();
                string email = Request.Cookies["user"].Value;

                var user = await db.Users.Where(u => u.user_email == email)
                            .FirstOrDefaultAsync();

                if (user != null)
                {
                    var post = new Post
                    {
                        post_content = message,
                        post_date = DateTime.Now,
                        post_topic = topic,
                        post_by = user.Id
                    };

                    db.Posts.Add(post);
                    await db.SaveChangesAsync();

                    var self = await db.Posts.Where(m => m.post_content == message)
                               .FirstOrDefaultAsync();

                    if (file != null)
                    {
                        var ms = new MemoryStream();
                        await file.InputStream.CopyToAsync(ms);

                        var data = new PostFile
                        {
                            file_name = file.FileName,
                            file_content = ms.ToArray(),
                            file_type = file.ContentType,
                            ref_id = self.post_id
                        };

                        db.PostFiles.Add(data);
                        await db.SaveChangesAsync();
                        return new HttpStatusCodeResult(HttpStatusCode.Created);
                    }
                }
                
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "You must to login!");
            }
            
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Something is wrong!");
        }

        [HttpDelete]
        public async Task<ActionResult> RemovePost(int? id)
        {
            if(id != null)
            {
                var db = new ForumEntity();
                var post = await db.Posts.Where(p => p.post_id == id.Value)
                            .FirstOrDefaultAsync();

                if(post != null)
                {
                    var files = await db.PostFiles.Where(f => f.ref_id == post.post_id)
                        .ToListAsync();

                    if (files.Count != 0)
                        db.PostFiles.RemoveRange(files);

                    db.Posts.Remove(post);
                    await db.SaveChangesAsync();
                }

                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Post has been not found!");
            }
            
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Select post!");
        }
    }
}