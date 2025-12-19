using System;
using System.Management.Automation;

namespace ThinktankApp
{
    public class TTState : TTObject
    {
        private string _value;
        private string _description;
        private string _from;

        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public string From
        {
            get { return _from; }
            set { SetProperty(ref _from, value); }
        }
        
        // Scripts can be ScriptBlock or String (for Default/Value)
        public object Default { get; set; }
        public object Test { get; set; }
        public object Apply { get; set; }
        public object Watch { get; set; }

        public TTState() : base()
        {
            Value = "";
            Description = "";
            From = "";
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
            if (From != null && From.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }
}
