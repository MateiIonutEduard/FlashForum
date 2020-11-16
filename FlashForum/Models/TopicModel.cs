using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlashForum.Models
{
    public class TopicModel
    {
        public int id { get; set; }
        public string subject { get; set; }
        public string date { get; set; }
        public int status { get; set; }
        public int userid { get; set; }
        public bool action { get; set; }
    }
}