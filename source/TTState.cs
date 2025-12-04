using System;
using System.Management.Automation;

namespace ThinktankApp
{
    public class TTState : TTObject
    {
        public string Value { get; set; }
        public string Description { get; set; }
        
        // Scripts can be ScriptBlock or String (for Default/Value)
        public object Default { get; set; }
        public object Test { get; set; }
        public object Apply { get; set; }
        public object Watch { get; set; }

        public TTState() : base()
        {
            Value = "";
            Description = "";
            Default = null;
            Test = null;
            Apply = null;
            Watch = null;
        }

        public override bool Matches(string keyword)
        {
            if (base.Matches(keyword)) return true;
            if (Value != null && Value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (Description != null && Description.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }
}
