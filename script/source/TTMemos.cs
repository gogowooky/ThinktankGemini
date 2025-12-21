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
            _itemDisplayColumns = "ID:メモID,UpdateDate:更新日,Name:タイトル,_flag:編集";
            _itemNarrowProperties = "ID,Name";
            _itemWideProperties = "_flag,ID,UpdateDate,Name";
            _itemMinimalProperties = "ID,Name";
            _itemSaveProperties = "UpdateDate,ID,Name";
        }
    }
}
