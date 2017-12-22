using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOIapp.Classes
{
    class CourseItem
    {
        public int CourseItemID { get; set; }

        public int CourseID { get; set; }

        public int CategoryID { get; set; }

        public string Name { get; set; }

        public double Points { get; set; }

        public double MinPoints { get; set; }

        public double CurrentPoints { get; set; }
    }
}
