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
    }
}