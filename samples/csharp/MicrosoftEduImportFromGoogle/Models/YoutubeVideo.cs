using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftEduImportFromGoogle.Models
{
    internal class YoutubeVideo: Material
    {
        public string Id { get; set; }
        public string Title { get; set; }   
        public string AlternateLink { get; set; }
    }
}
