using System;

namespace ThinktankApp
{
    public class TTMemo : TTObject
    {
        private string _flagValue;
        private string _keywords;

        /// <summary>
        /// 編集フラグ（DataGridのバインディング用）
        /// </summary>
        public string _flag
        {
            get { return _flagValue; }
            set { SetProperty(ref _flagValue, value); }
        }

        /// <summary>
        /// キーワード
        /// </summary>
        public string Keywords
        {
            get { return _keywords; }
            set { SetProperty(ref _keywords, value); }
        }

        public TTMemo() : base()
        {
            _flag = "";
            Keywords = "";
        }

        public override bool Matches(string keyword)
        {
            if (base.Matches(keyword)) return true;
            if (Keywords != null && Keywords.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }
}
