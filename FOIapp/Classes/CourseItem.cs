using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FOIapp.Annotations;

namespace FOIapp.Classes
{
    public class CourseItem : INotifyPropertyChanged
    {
        private double currentPoints;
        public int CourseItemID { get; set; }

        public int CourseID { get; set; }

        public int CategoryID { get; set; }

        public string Name { get; set; }

        public double Points { get; set; }

        public double MinPoints { get; set; }

        public double CurrentPoints
        {
            get { return currentPoints; }
            set
            {
                currentPoints = value;
                OnPropertyChanged(nameof(CurrentPoints));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
