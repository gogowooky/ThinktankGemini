using System;

namespace ThinktankApp
{
    public class TTWebSearch : TTObject
    {
        private string _url;
        private object _script;

        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        public object Script
        {
            get { return _script; }
            set { SetProperty(ref _script, value); }
        }

        public TTWebSearch() : base()
        {
            ID = "Web";
            Name = "Web検索";
            Url = "";
            Script = null;
        }
    }
}
