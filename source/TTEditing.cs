using System;

namespace ThinktankApp
{
    public class TTEditing : TTObject
    {
        private long _caretPos;
        private bool _wordWrap;
        private string _foldings;

        public long CaretPos
        {
            get { return _caretPos; }
            set { SetProperty(ref _caretPos, value); }
        }

        public bool WordWrap
        {
            get { return _wordWrap; }
            set { SetProperty(ref _wordWrap, value); }
        }

        public string Foldings
        {
            get { return _foldings; }
            set { SetProperty(ref _foldings, value); }
        }

        public TTEditing() : base()
        {
            CaretPos = 1;
            WordWrap = false;
            Foldings = "";
        }
    }
}
