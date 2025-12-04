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

        public override bool Matches(string keyword)
        {
            if (base.Matches(keyword)) return true;
            if (Context != null && Context.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (Mods != null && Mods.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (Key != null && Key.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }
}
