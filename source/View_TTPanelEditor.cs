using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;

namespace ThinktankApp
{
    public class TTPanelEditor : TTPanelBase
    {
        public DockPanel EditorPanel { get; private set; }
        public TextEditor EditorKeyword { get; private set; }
        public TextEditor EditorMain { get; private set; }

        public TTPanelEditor(string name, string xamlPath, string stylePath, TTModels models) 
            : base(name, xamlPath, stylePath, models)
        {
        }

        protected override void LoadView(string xamlPath, string stylePath)
        {
            base.LoadView(xamlPath, stylePath);
            if (View != null)
            {
                EditorPanel = (DockPanel)View.FindName("EditorPanel");
                EditorKeyword = (TextEditor)View.FindName("EditorKeyword");
                EditorMain = (TextEditor)View.FindName("EditorMain");
            }
        }

        public override void Setup()
        {
            base.Setup();

            if (EditorMain != null)
            {
                EditorMain.ShowLineNumbers = true;
                EditorMain.WordWrap = true;
                EditorMain.FontFamily = new System.Windows.Media.FontFamily("Meiryo");
                EditorMain.FontSize = 12;
                EditorMain.GotFocus += (s, e) => OnFocusChanged("Editor", "Main");
            }

            if (EditorKeyword != null)
            {
                EditorKeyword.ShowLineNumbers = false;
                EditorKeyword.WordWrap = false;
                EditorKeyword.FontFamily = new System.Windows.Media.FontFamily("Meiryo");
                EditorKeyword.FontSize = 12;
                EditorKeyword.GotFocus += (s, e) => OnFocusChanged("Editor", "Keyword");
            }
        }

        public virtual void SetFontSize(string size)
        {
            double fontSize;
            if (double.TryParse(size, out fontSize))
            {
                if (EditorMain != null) EditorMain.FontSize = fontSize;
            }
        }

        public override string GetMode()
        {
            if (EditorPanel != null && EditorPanel.Visibility == Visibility.Visible) return "Editor";
            return base.GetMode();
        }
    }
}
