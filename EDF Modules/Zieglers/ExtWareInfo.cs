using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace Zieglers
{
    public class ExtWareInfo : WareInfo
    {
        public int Page { get; set; }

        public string Url { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public string Title { get; set; }

        public double Price { get; set; }

        public double Weight { get; set; }

        public new string Description { get; set; }

        public string Images { get; set; }
         
    }
}
