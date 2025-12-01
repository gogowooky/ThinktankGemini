using System;

namespace ThinktankApp
{
    public class TTEvent : TTObject
    {
        public string Context { get; set; }
        public string Mods { get; set; }
        public string Key { get; set; }

        public TTEvent() : base()
        {
            Context = "";
            Mods = "";
            Key = "";
        }
    }
}
