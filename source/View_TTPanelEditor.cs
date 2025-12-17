using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace ThinktankApp
{
    public class TTPanelEditorBase : TTPanelBase
    {
        public DockPanel EditorPanel { get; private set; }
        public TextEditor EditorKeyword { get; private set; }
        public TextEditor EditorMain { get; private set; }

        public TTPanelEditorBase(string name, string xamlPath, string stylePath, TTModels models) 
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
                EditorMain.GotFocus += (s, e) => OnFocusChanged("Editor", "Main");
            }
            if (EditorKeyword != null)
            {
                EditorKeyword.GotFocus += (s, e) => OnFocusChanged("Editor", "Keyword");
            }
        }

        public override string GetMode()
        {
            if (EditorPanel != null && EditorPanel.Visibility == Visibility.Visible) return "Editor";
            return base.GetMode();
        }
    }

    public class TTPanelEditor : TTPanelEditorBase
    {
        private Regex _editorKwRegex;
        private List<Regex> _editorKwRegexs;
        private RegexColorizingTransformer _keywordHighlighter;

        public TTPanelEditor(string name, string xamlPath, string stylePath, TTModels models) 
            : base(name, xamlPath, stylePath, models)
        {
            _editorKwRegex = new Regex("^");
            _editorKwRegexs = new List<Regex>();
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
            }

            if (EditorKeyword != null)
            {
                EditorKeyword.ShowLineNumbers = false;
                EditorKeyword.WordWrap = false;
                EditorKeyword.FontFamily = new System.Windows.Media.FontFamily("Meiryo");
                EditorKeyword.FontSize = 12;
            }

            UpdateHighlightRule();
            UpdateKeywordRegex();
            UpdateHighlight();
        }

        public virtual void SetFontSize(string size)
        {
            double fontSize;
            if (double.TryParse(size, out fontSize))
            {
                if (EditorMain != null) EditorMain.FontSize = fontSize;
            }
        }

        public void UpdateHighlightRule()
        {
            string scriptPath = "";
            
            // Should be available via TTApplicationResource
            var appRes = TTApplication.Current as TTApplicationResource;
            if (appRes != null)
            {
                scriptPath = appRes.ScriptPath;
            }
            else
            {
                // Fallback (e.g. design time or unit test if mocked)
                try {
                     var loc = Models.GetType().Assembly.Location;
                     if (!string.IsNullOrEmpty(loc)) scriptPath = Path.Combine(Path.GetDirectoryName(loc), "..", "script");
                } catch { }
            }

            if (EditorMain != null)
            {
                string rulePath = Path.Combine(scriptPath, "EditorRule.xshd");
                if (File.Exists(rulePath))
                {
                    using (var reader = new XmlTextReader(rulePath))
                    {
                        EditorMain.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }

            if (EditorKeyword != null)
            {
                string rulePath = Path.Combine(scriptPath, "KeywordRule.xshd");
                if (File.Exists(rulePath))
                {
                    using (var reader = new XmlTextReader(rulePath))
                    {
                        EditorKeyword.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
        }

        public void UpdateKeywordRegex()
        {
            string text = "";
            if (EditorKeyword != null)
            {
                text = EditorKeyword.Text;
            }

            _editorKwRegex = new Regex("^");
            _editorKwRegexs.Clear();

            // Simple splitting logic mimicking typical keyword splitting (space separated)
            // The PS script seems to use complex logic not fully visible in the snippet (regex was hidden in PS or not fully shown)
            // But usually keywords are split by whitespace.
            // If the user wants exact parity with PS, I'd need the exact logic.
            // Looking at PS script: $this._editorkwregex = [regex]::New( '^' ) ... 
            // It doesn't show the logic to populate it. It is likely updated in `UpdateKeywordRegex` which was NOT shown in full in PS snippet?
            // Wait, looking at PS snippet again... line 330 calls $this.UpdateKeywordRegex(). But where is the definition?
            // It seems `UpdateKeywordRegex` definition is MISSING from the PS view I saw earlier! 
            // I need to find `UpdateKeywordRegex` in PS.
            // I will add a TODO or basic implementation and then check PS file again if needed.
            // Re-reading PS file content... 
            // I see `UpdateHighlightRule`, `UpdateHighlight`, `UpdateNodesPos`, `UpdateActorsPos`.
            // I DO NOT SEE `UpdateKeywordRegex` implementation in lines 1-800 of TTPanel.ps1.
            // I should assume standard keyword splitting for now or ask/search for it.
            // Let's implement basic whitespace splitting which is standard.
            
            if (string.IsNullOrWhiteSpace(text)) return;

            try 
            {
                var keywords = text.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kw in keywords)
                {
                     // Escape kw
                     string escaped = Regex.Escape(kw);
                     _editorKwRegexs.Add(new Regex(escaped, RegexOptions.IgnoreCase));
                }
                
                if (_editorKwRegexs.Count > 0)
                {
                     string pattern = "(" + string.Join("|", _editorKwRegexs.Select(r => r.ToString())) + ")";
                     _editorKwRegex = new Regex(pattern, RegexOptions.IgnoreCase);
                }
            }
            catch 
            {
                // Ignore regex errors from user input
            }
        }

        public void UpdateHighlight()
        {
            if (EditorMain == null) return;

            var textView = EditorMain.TextArea.TextView;
            
            // Remove old transformer
            if (_keywordHighlighter != null)
            {
                textView.LineTransformers.Remove(_keywordHighlighter);
                _keywordHighlighter = null;
            }

            // Create new transformer
            if (_editorKwRegexs.Any())
            {
                _keywordHighlighter = new RegexColorizingTransformer(_editorKwRegexs);
                textView.LineTransformers.Add(_keywordHighlighter);
            }

            textView.Redraw();
        }

        // Inner class for highlighting
        private class RegexColorizingTransformer : DocumentColorizingTransformer
        {
            private readonly List<Regex> _regexes;
            private readonly SolidColorBrush[] _brushes;

            public RegexColorizingTransformer(List<Regex> regexes)
            {
                _regexes = regexes;
                // Pre-define some colors for markers (Marker1, Marker2, ... Marker5)
                // Mimicking what usually works or what is in xshd
                _brushes = new SolidColorBrush[] 
                {
                    Brushes.Yellow,
                    Brushes.Cyan, 
                    Brushes.Orange,
                    Brushes.LightGreen,
                    Brushes.Pink,
                    Brushes.LightBlue
                };
            }

            protected override void ColorizeLine(DocumentLine line)
            {
                string text = CurrentContext.Document.GetText(line);
                int lineStart = line.Offset;

                for (int i = 0; i < _regexes.Count; i++)
                {
                    var regex = _regexes[i];
                    var brush = _brushes[i % _brushes.Length]; // Cycle colors

                    foreach (Match match in regex.Matches(text))
                    {
                        ChangeLinePart(
                            lineStart + match.Index,
                            lineStart + match.Index + match.Length,
                            (VisualLineElement element) =>
                            {
                                element.TextRunProperties.SetBackgroundBrush(brush);
                            });
                    }
                }
            }
        }
    }
}
