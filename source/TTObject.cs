using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThinktankApp
{
    public class TTObject : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _updateDate;
        private TTObject _parent;

        public string ID
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string UpdateDate
        {
            get { return _updateDate; }
            set { SetProperty(ref _updateDate, value); }
        }

        public TTObject Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public TTObject()
        {
            ID = "";
            Name = "";
            UpdateDate = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
