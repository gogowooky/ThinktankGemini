using System;

namespace ThinktankApp
{
    public class TTModels : TTCollection
    {
        public TTStatus Status { get; private set; }
        public TTActions Actions { get; private set; }
        public TTEvents Events { get; private set; }
        public TTMemos Memos { get; private set; }

        public TTModels() : base()
        {
            ID = "Thinktank";
            Name = "Thinktank";
            Description = "コレクション一覧";

            Status = new TTStatus();
            Status.ID = "Status";
            Status.Name = "ステータス";

            Actions = new TTActions();
            Actions.ID = "Actions";
            Actions.Name = "アクション";

            Events = new TTEvents();
            Events.ID = "Events";
            Events.Name = "イベント";

            Memos = new TTMemos();

            AddItem(Status);
            AddItem(Actions);
            AddItem(Events);
            AddItem(Memos);
        }

        public TTCollection GetCollection(string id)
        {
            if (id == ID)
            {
                return this;
            }
            return GetItem(id) as TTCollection;
        }
    }
}
