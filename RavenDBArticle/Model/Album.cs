using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavenDBArticle.Model
{
    public class Album
    {
        public string AlbumArtUrl { get; set; }

        public int CountSoldPrice { get; set; }

        public double Price { get; set; }

        public string Title { get; set; }
    }
}
