using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream_Music_MAUI.Classes
{
    public class Entry
    {
        public string _type { get; set; }
        public string ie_key { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public object description { get; set; }
        public int? duration { get; set; }
        public string channel_id { get; set; }
        public string channel { get; set; }
        public string channel_url { get; set; }
        public string uploader { get; set; }
        public object uploader_id { get; set; }
        public object uploader_url { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public object timestamp { get; set; }
        public object release_timestamp { get; set; }
        public object availability { get; set; }
        public long? view_count { get; set; }
        public object live_status { get; set; }
        public object channel_is_verified { get; set; }
        public object __x_forwarded_for_ip { get; set; }
    }

    public class FilesToMove
    {
    }

    public class PlayerList
    {
        public string id { get; set; }
        public string title { get; set; }
        public object availability { get; set; }
        public object channel_follower_count { get; set; }
        public string description { get; set; }
        public List<object> tags { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public string modified_date { get; set; }
        public long view_count { get; set; }
        public int playlist_count { get; set; }
        public string channel { get; set; }
        public string channel_id { get; set; }
        public object uploader_id { get; set; }
        public string uploader { get; set; }
        public string channel_url { get; set; }
        public object uploader_url { get; set; }
        public string _type { get; set; }
        public List<Entry> entries { get; set; }
        public string extractor_key { get; set; }
        public string extractor { get; set; }
        public string webpage_url { get; set; }
        public string original_url { get; set; }
        public string webpage_url_basename { get; set; }
        public string webpage_url_domain { get; set; }
        public object release_year { get; set; }
        public int epoch { get; set; }
        public FilesToMove __files_to_move { get; set; }
        public Version _version { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string id { get; set; }
        public string resolution { get; set; }
    }

    public class Version
    {
        public string version { get; set; }
        public object current_git_head { get; set; }
        public string release_git_head { get; set; }
        public string repository { get; set; }
    }

}
