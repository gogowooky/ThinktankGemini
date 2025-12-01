using System;
using System.Linq;

namespace ThinktankApp
{
    public class TTStatus : TTCollection
    {
        public TTStatus() : base()
        {
            Description = "Status";
        }

        public void SetValue(string id, string value)
        {
            var item = GetItem(id) as TTState;
            if (item != null)
            {
                if (item.Value != value)
                {
                    item.Value = value;
                    // NotifyUpdated logic could be added here if needed
                }
            }
        }
    }
}
