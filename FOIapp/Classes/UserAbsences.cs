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
    public class AbsenceItem : INotifyPropertyChanged
    {
        private int timesAbsent;

        public int AbsenceCategoryID { get; set; }
        public string AbsenceCategory { get; set; }

        public int MaxTimesAbsent { get; set; }

        public int TimesAbsent
        {
            get { return timesAbsent; }

            set
            {
                timesAbsent = value;
                OnPropertyChanged(nameof(TimesAbsent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //// Create the OnPropertyChanged method to raise the event
        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

    }
}
