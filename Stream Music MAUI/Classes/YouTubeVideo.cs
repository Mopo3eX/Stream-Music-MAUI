using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream_Music_MAUI.Classes
{
    public class YouTubeVideo
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public int? Duration { get; set; }
        public override string ToString()
        {
            return $"{Title} | {Url}";
        }
    }
}
