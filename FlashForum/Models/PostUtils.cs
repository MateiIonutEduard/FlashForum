using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;

namespace FlashForum.Models
{
    public class PostUtils
    {
        public static string LoadPost(string data)
        {
            string str = @"(http[s]|ftp[s])?:(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\(\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+";
            var all = Regex.Matches(data, str);
            string result = data;

            foreach(var item in all)
                result = Regex.Replace(result, str, $"<a href='{item}'>{item}</a>");
            
            return result;
        }

        public static (int, int) GetVotes(int id)
        {
            var db = new ForumEntity();
            var post = db.Posts.Where(m => m.post_id == id)
                    .FirstOrDefault();

            if(post != null)
            {
                var likes = db.Likes.Where(v => v.pid == id)
                    .ToList();

                int up = likes.Where(u => u.like).Count();
                return (up, likes.Count() - up);
            }

            return (0, 0);
        }

        public static (string, string) UserLike(int uid, int pid)
        {
            var db = new ForumEntity();
            var msg = db.Posts.Where(m => m.post_id == pid)
                    .FirstOrDefault();

            var like = db.Likes.Where(v => v.uid == uid && v.pid == pid)
                    .FirstOrDefault();

            if(like != null)
            {
                var up = like.like ? "fa-thumbs-up" : "fa-thumbs-o-up";
                var down = !like.like ? "fa-thumbs-down" : "fa-thumbs-o-down";
                return (up, down);
            }

            return ("fa-thumbs-o-up", "fa-thumbs-o-down");
        }
    }
}