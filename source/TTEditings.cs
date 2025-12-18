using System;

namespace ThinktankApp
{
    public class TTEditings : TTCollection
    {
        public TTEditings() : base()
        {
            Description = "Editings";
            _itemDisplayColumns = "ID:メモID,UpdateDate:更新日,CaretPos:カレット位置,WordWrap:ワードラップ,Foldings:折りたたみ";
            _itemNarrowProperties = "ID,UpdateDate";
            _itemWideProperties = "ID,UpdateDate,CaretPos,WordWrap,Foldings";
            _itemMinimalProperties = "ID,UpdateDate";
            _itemSaveProperties = "ID,UpdateDate,CaretPos,WordWrap,Foldings";
        }
    }
}
