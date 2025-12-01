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
    }
}
