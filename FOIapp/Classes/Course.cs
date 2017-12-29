using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOIapp.Classes
{
    public class Course
    {
        public int CourseID { get; set; }
        public string Name { get; set; }

        public string ShortName { get; set; }

        public int StudyYear { get; set; }

        public string StudyName { get; set; }

        public string CourseColor { get; set; }

        public string ShortInfo => $"{StudyName}, {StudyYear}. godina";
    }
}
