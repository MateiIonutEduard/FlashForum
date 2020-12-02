using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Data.Entity;
using System.IO;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace FlashForum.Models
{
    public class UserService
    {
        ForumEntity db;

        public UserService()
        {
            db = new ForumEntity();
        }

        public async Task<bool> Login(string email, string password)
        {
            var hash = SHA256.Create();
            var db = new ForumEntity();

            byte[] hashKey = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            string key = Convert.ToBase64String(hashKey);

            var user = await db.Users.Where(u => u.user_email == email && u.user_pass == key).FirstOrDefaultAsync();
            return (user != null);
        }

        public async Task<bool> Signup(string username, string password, string email, bool level, HttpPostedFileBase file)
        {
            var hash = SHA256.Create();
            byte[] hashKey = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            string key = Convert.ToBase64String(hashKey);

            var exists = await db.Users.Select(u => u.user_email == email || u.user_name == username)
                        .FirstOrDefaultAsync();

            if(!exists)
            {
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

                var user = db.Users.Where(item => item.user_email == node.user_email && item.user_pass == key)
                        .FirstOrDefault();
                
                if(file != null)
                {
                    var ms = new MemoryStream();
                    await file.InputStream.CopyToAsync(ms);

                    var profile = new Profile
                    {
                        image_name = Path.GetFileName(file.FileName),
                        image_type = file.ContentType,
                        image_content = ms.ToArray(),
                        user_id = user.Id
                    };

                    db.Profiles.Add(profile);
                    await db.SaveChangesAsync();
                }
            }

            return exists;
        }

        public User GetUser(string email)
        {
            var user = db.Users.Where(u => u.user_email == email)
                        .FirstOrDefault();

            return user;
        }

        public static void SendEmail(string to, string subject, string body)
        {
            string server = ConfigurationManager.AppSettings["host"];
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);

            string client = ConfigurationManager.AppSettings["client"];
            string secret = ConfigurationManager.AppSettings["secret"];

            SmtpClient host = new SmtpClient(server, port);
            host.EnableSsl = true;
            host.UseDefaultCredentials = false;
            host.Credentials = new NetworkCredential(client, secret);

            MailMessage mail = new MailMessage();

            mail.To.Add(to);
            mail.From = new MailAddress(client);
            mail.Body = body;
            host.Send(mail);
        }

        public bool HasPrivilegies(string cookie)
        {
            var user = db.Users.Where(u => u.user_email == cookie).FirstOrDefault();
            return (user != null) && user.user_level;
        }
    }
}