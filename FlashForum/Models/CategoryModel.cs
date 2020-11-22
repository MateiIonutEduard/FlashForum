using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlashForum.Models
{
    public class CategoryModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string content { get; set; }
        public string date { get; set; }
    }
}