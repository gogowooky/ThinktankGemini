using System;
using System.Reflection;

namespace ThinktankApp
{
    public class TTWebSearches : TTCollection
    {
        public TTWebSearches() : base()
        {
            Description = "Webサーチ"; 
            _itemDisplayColumns = "ID:WebサーチID,Name:Webサーチ名,Url:URL";
            _itemNarrowProperties = "ID,Url";
            _itemWideProperties = "Name,ID,Url";
            _itemMinimalProperties = "ID,Url";
            _itemSaveProperties = "ID,Name,Url";
        }

        public string CreateUrlFromTag(object match)
        {
            // [MOCKED] problematic logic commented out
            /*
            try
            {
                Type type = match.GetType();
                PropertyInfo propTag = type.GetProperty("tag");
                PropertyInfo propKeywords = type.GetProperty("keywords");

                string tag = (string)propTag?.GetValue(match, null);
                string keywords = (string)propKeywords?.GetValue(match, null);
                
                if (tag == null) tag = (string)type.GetProperty("Tag")?.GetValue(match, null);
                if (keywords == null) keywords = (string)type.GetProperty("Keywords")?.GetValue(match, null);

                if (tag == null) tag = "";
                if (keywords == null) keywords = "";

                var websearch = GetItem(tag) as TTWebSearch;
                if (websearch == null) return "";

                string url = websearch.Url;
                
                if (websearch.Script == null) 
                {
                    return url.Replace("<keywords>", Uri.EscapeDataString(keywords));
                }

                if (websearch.Script is System.Management.Automation.ScriptBlock sb)
                {
                     // Invoke with the match object
                     var results = sb.Invoke(match);
                     if (results != null && results.Count > 0)
                     {
                        return results[0].ToString();
                     }
                     return "";
                }

                return url.Replace("<keywords>", Uri.EscapeDataString(keywords));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return "";
            }
            */
            return "";
        }
    }
}
