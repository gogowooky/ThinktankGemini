using System;

namespace ThinktankApp
{
    public class TTEvent : TTObject
    {
        public string Tag { get; set; }
        public string Mods { get; set; }
        public string Key { get; set; }

        public TTEvent() : base()
        {
            Tag = "";
            Mods = "";
            Key = "";
        }
    }
}
