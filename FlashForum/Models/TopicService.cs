using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlashForum.Models
{
    public class TopicService
    {
        ForumEntity db;

        public TopicService()
        {
            db = new ForumEntity();
        }

        public List<TopicModel> AllTopics(int id, int? filter)
        {
            var data = new List<TopicModel>();
            var list = db.Topics.ToList();

            if(filter != null && filter.Value > 1)
            {
                if (filter.Value == 2)
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
                        userid = node.topic_by
                    }).ToList();

                    return data;
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
                            userid = node.topic_by
                        }).ToList();

                    return data;
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
                    userid = node.topic_by
                }).ToList();

                return data;
            }
        }
    }
}