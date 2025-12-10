using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace ThinktankApp
{
    public class TTBasePanel
    {
        public string Name { get; private set; }
        public UserControl View { get; private set; }
        public Label Title { get; private set; }
        public ContextMenu Menu { get; private set; }
        public TTModels Models { get; private set; }

        public event Action<string, string, string> FocusChanged;

        public TTBasePanel(string name, string xamlPath, string stylePath, TTModels models)
        {
            Name = name;
            Models = models;
            LoadView(xamlPath, stylePath);
        }

        protected virtual void LoadView(string xamlPath, string stylePath)
        {
            try
            {
                string xamlContent = File.ReadAllText(xamlPath);
                string styleContent = File.ReadAllText(stylePath);
                
                // Replace ResourceDictionary with style content
                xamlContent = xamlContent.Replace("<ResourceDictionary Source=\"Style.xaml\" />", styleContent);

                using (var sr = new StringReader(xamlContent))
                using (var xmlReader = XmlReader.Create(sr))
                {
                    View = (UserControl)XamlReader.Load(xmlReader);
                }

                // Find common controls
                Title = (Label)View.FindName("Title");
                Menu = View.ContextMenu;

                if (Title != null)
                {
                    Title.Content = Name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error loading TTPanel '{0}': {1}\n{2}", Name, ex.Message, ex.StackTrace));
                throw;
            }
        }

        protected bool _isFocused = false;

        public virtual void Setup()
        {
            // Base setup if needed
        }

        protected void OnFocusChanged(string mode, string tool)
        {
            if (FocusChanged != null)
            {
                FocusChanged(Name, mode, tool);
            }
        }

        public virtual string GetMode()
        {
            return "";
        }

        protected virtual string GetTitleSuffix()
        {
            return "";
        }

        protected virtual void UpdateTitle()
        {
            if (Title == null) return;

            string titleText = Name;
            titleText += GetTitleSuffix();

            if (_isFocused)
            {
                titleText = "●" + titleText;
            }

            Title.Content = titleText;
        }

        public void SetFocusIndicator(bool isFocused)
        {
            _isFocused = isFocused;
            UpdateTitle();
        }

        public void SetTitleVisible(string visible)
        {
            if (Title == null) return;
            bool isVisible = visible.ToLower() == "true";
            Title.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
