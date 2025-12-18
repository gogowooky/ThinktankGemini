using System;

namespace ThinktankApp
{
    public class TTWebLink : TTObject
    {
        private string _url;

        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        public TTWebLink() : base()
        {
            ID = "WebLink";
            Name = "Webリンク";
            Url = "";
        }
    }
}
