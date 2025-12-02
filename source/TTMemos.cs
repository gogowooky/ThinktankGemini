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
            _itemDisplayColumns = "ID:メモID,UpdateDate:更新日,Name:タイトル,_flag:編";
            _itemNarrowProperties = "_flag,UpdateDate,ID,Name";
            _itemWideProperties = "_flag,UpdateDate,ID,Name";
            _itemMinimalProperties = "Name";
            _itemSaveProperties = "_flag,UpdateDate,ID,Name";
        }
    }
}
