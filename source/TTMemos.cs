using System;

namespace ThinktankApp
{
    public class TTMemos : TTCollection
    {
        public TTMemos() : base()
        {
            ID = "Memos";
            Name = "メモ";
            Description = "メモ一覧";
        }
    }
}
