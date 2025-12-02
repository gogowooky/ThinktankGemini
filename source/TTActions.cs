using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace ThinktankApp
{
    public class TTActions : TTCollection
    {
        private Dictionary<string, TTAction> _actionMap;

        public TTActions() : base()
        {
            _actionMap = new Dictionary<string, TTAction>();
            Description = "Actions";
            _itemDisplayColumns = "ID:ID,Name:名前";
            _itemNarrowProperties = "ID,Name";
            _itemWideProperties = "ID,Name";
            _itemMinimalProperties = "ID,Name";
            _itemSaveProperties = "ID,Name";
        }

        public void AddItem(TTAction action)
        {
            if (!_actionMap.ContainsKey(action.ID))
            {
                _actionMap[action.ID] = action;
                base.AddItem(action);
            }
        }

        public new TTAction GetItem(string id)
        {
            if (_actionMap.ContainsKey(id))
            {
                return _actionMap[id];
            }
            return null;
        }

        public bool Invoke(string id, object tag, Runspace runspace)
        {
            var action = GetItem(id);
            if (action != null)
            {
                return action.Invoke(tag, runspace);
            }
            return false;
        }
    }
}
