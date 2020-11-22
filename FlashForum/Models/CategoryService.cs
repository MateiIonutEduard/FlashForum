using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Data.Entity;

namespace FlashForum.Models
{
    public class CategoryService
    {
        private ForumEntity db;

        public CategoryService()
        {
            db = new ForumEntity();
        }

        public async Task<bool> CreateCategory(string name, string content)
        {
            var item = await db.Categories.Where(node => node.cat_name == name).FirstOrDefaultAsync();

            if (item != null) return false;
            else
            {
                Category newItem = new Category
                {
                    cat_name = name,
                    cat_description = content,
                    cat_date = DateTime.Now
                };

                db.Categories.Add(newItem);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<int> DeleteCategory(int? id)
        {
            if(id != null)
            {
                var item = await db.Categories.Where(node => node.cat_id == id.Value)
                            .FirstOrDefaultAsync();

                if (item != null)
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
                    return 1;
                }
                else return -1;
            }

            return 0;
        }

        public List<CategoryModel> AllCategories(int? filter)
        {
            var data = new List<CategoryModel>();
            var list = db.Categories.ToList();

            if (filter != null && filter.Value > 1)
            {
                if(filter == 2)
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
                    }).ToList();

                    return data;
                }
                else
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
                        date = node.cat_date.ToString("MM/dd/yyyy HH:mm:ss")
                    }).ToList();

                    return data;
                }
            }

            data = list.Select(c => new CategoryModel
            {
                id = c.cat_id,
                name = c.cat_name,
                content = c.cat_description,
                date = c.cat_date.ToString("MM/dd/yyyy HH:mm:ss")
            }).ToList();

            return data;
        }
    }
}