using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOIapp.Classes
{
    public class CourseCategory
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public List<CourseItem> ChildItems { get; set; }

        public CourseCategory()
        {
            ChildItems = new List<CourseItem>();
        }
    }
}
