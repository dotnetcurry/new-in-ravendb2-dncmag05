using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavenDBArticle.Model
{
    public class Order
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string[] ProductIds { get; set; }
    }
}
