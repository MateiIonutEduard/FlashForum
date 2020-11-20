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

        public async Task<ActionResult> Categories(int? filter)
        {
            var db = new ForumEntity();
            var list = await db.Categories.ToListAsync();
            var data = new List<CategoryModel>();

            var email = Request.Cookies["user"] != null ? Request.Cookies["user"].Value : "";
            var user = await db.Users.Where(u => u.user_email == email).FirstOrDefaultAsync();
            bool can = (user != null) && user.user_level;

            if (filter != null && filter.Value > 1)
            {
                if (filter.Value == 3)
                {
                    list.Sort((left, right) =>
                    {
                        return right.cat_date.CompareTo(left.cat_date);
                    });

                    data = list.Select(node =>
                    new CategoryModel
                    {
                        id = node.cat_id,
                        name = node.cat_name,
                        content = node.cat_description,
                        date = node.cat_date.ToString("MM/dd/yyyy HH:mm:ss"),
                        action = can
                    }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    list.Sort((left, right) =>
                    {
                        int lcount = db.Topics.Count(t => t.topic_cat == left.cat_id);
                        int rcount = db.Topics.Count(t => t.topic_cat == right.cat_id);
                        return rcount.CompareTo(lcount);
                    });

                    data = list.Select(node =>
                    new CategoryModel
                    {
                        id = node.cat_id,
                        name = node.cat_name,
                        content = node.cat_description,
                        date = node.cat_date.ToString("MM/dd/yyyy HH:mm:ss"),
                        action = can
                    }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                data = list.Select(node =>
                  new CategoryModel
                  {
                      id = node.cat_id,
                      name = node.cat_name,
                      content = node.cat_description,
                      date = node.cat_date.ToString("MM/dd/yyyy HH:mm:ss"),
                      action = can
                  }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
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
            var item = await db.Categories.Where(node => node.cat_name == name).FirstOrDefaultAsync();

            if (item != null)
            {
                table.Add("message", "Category was created once!");
                table.Add("success", false);
                return Json(table, JsonRequestBehavior.AllowGet);
            }

            Category newItem = new Category
            {
                cat_name = name,
                cat_description = content,
                cat_date = DateTime.Now
            };

            db.Categories.Add(newItem);
            await db.SaveChangesAsync();

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

            var item = await db.Categories.Where(node => node.cat_id == id.Value)
                .FirstOrDefaultAsync();

            if(item != null)
            {
                // remove category first...
                db.Categories.Remove(item);

                // find topics
                var topics = await db.Topics.Where(node => node.topic_cat == id.Value).ToListAsync();

                if (topics.Count > 0)
                {
                    topics.ForEach((node) =>
                    {
                        var posts = db.Posts.Where(post => post.post_topic == node.topic_id).ToList();
                        if (posts.Count > 0)
                        {
                            // delete the files corresponding to the posts
                            posts.ForEach((post) =>
                                {
                                    var files = db.PostFiles.Where(file => file.ref_id == post.post_id).ToList();
                                    if (files.Count != 0) db.PostFiles.RemoveRange(files);
                                });

                            // drop the posts using topics id
                            db.Posts.RemoveRange(posts);
                        }
                    });

                    // finally, remove these topics
                    db.Topics.RemoveRange(topics);
                }

                await db.SaveChangesAsync();

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

        public async Task<ActionResult> ViewTopics(int id, int? filter)
        {
            var db = new ForumEntity();
            var data = new List<TopicModel>();
            var list = await db.Topics.ToListAsync();

            var email = Request.Cookies["user"] != null ? Request.Cookies["user"].Value : "";
            var user = await db.Users.Where(u => u.user_email == email).FirstOrDefaultAsync();

            if (filter != null)
            {
                int orderBy = filter.Value;

                if(orderBy == 1)
                {
                    // all threads
                    data = list.Where(node => node.topic_cat == id).Select(node => new TopicModel
                    {
                        id = node.topic_id,
                        subject = node.topic_subject,
                        date = node.topic_date.ToString("MM/dd/yyyy HH:mm:ss"),
                        status = node.status,
                        userid = node.topic_by,
                        action = user != null && (user.user_level || user.Id == node.topic_by)
                    }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else if(orderBy == 2)
                {
                    // most posts...
                    var t = list.Where(node => node.topic_cat == id)
                        .ToList();

                    t.Sort((left, right) =>
                    {
                        int lcount = db.Posts.Count(node => node.post_topic == left.topic_id);
                        int rcount = db.Posts.Count(node => node.post_topic == right.topic_id);
                        return rcount.CompareTo(lcount);
                    });

                    data = t.Select(node => new TopicModel
                    {
                        id = node.topic_id,
                        subject = node.topic_subject,
                        date = node.topic_date.ToString("MM/dd/yyyy HH:mm:ss"),
                        status = node.status,
                        userid = node.topic_by,
                        action = user != null && (user.user_level || user.Id == node.topic_by)
                    }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // active topics
                    data = list.Where(node => node.status < 3 && node.topic_cat == id)
                        .Select(node => new TopicModel
                        {
                            id = node.topic_id,
                            subject = node.topic_subject,
                            date = node.topic_date.ToString("MM/dd/yyyy HH:mm:ss"),
                            status = node.status,
                            userid = node.topic_by,
                            action = user != null && (user.user_level || user.Id == node.topic_by)
                        }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                // all threads
                data = list.Where(node => node.topic_cat == id).Select(node => new TopicModel
                {
                    id = node.topic_id,
                    subject = node.topic_subject,
                    date = node.topic_date.ToString("MM/dd/yyyy HH:mm:ss"),
                    status = node.status,
                    userid = node.topic_by,
                    action = user != null && (user.user_level || user.Id == node.topic_by)
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
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
            else return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User doesn't exists!");
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}