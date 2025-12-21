using System;
using System.Reflection;
using System.Collections;
using System.Management.Automation;
using System.Net;

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
            try
            {
                string tag = "";
                string keywords = "";

                if (match is IDictionary)
                {
                    IDictionary dict = (IDictionary)match;
                    if (dict.Contains("tag")) tag = dict["tag"] as string;
                    if (dict.Contains("keywords")) keywords = dict["keywords"] as string;
                }
                else
                {
                    // Fallback to reflection for properties (case-insensitive)
                    Type type = match.GetType();
                    PropertyInfo pTag = type.GetProperty("tag", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo pKw = type.GetProperty("keywords", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (pTag != null) tag = pTag.GetValue(match, null) as string;
                    if (pKw != null) keywords = pKw.GetValue(match, null) as string;
                }

                if (string.IsNullOrEmpty(tag)) return "";
                if (keywords == null) keywords = "";

                var websearch = GetItem(tag) as TTWebSearch;
                if (websearch == null) return "";

                if (websearch.Script != null && websearch.Script is ScriptBlock)
                {
                    ScriptBlock sb = (ScriptBlock)websearch.Script;
                    var results = sb.Invoke(match);
                    if (results != null && results.Count > 0)
                    {
                        return results[0].ToString();
                    }
                    return "";
                }

                string url = websearch.Url;
                if (string.IsNullOrEmpty(url)) return "";

                return url.Replace("<keywords>", WebUtility.UrlEncode(keywords));
            }
            catch
            {
                // Fail silently or log if possible
                return "";
            }
        }
    }
}
