using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCMHelper_MenuDesigner
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ValueOptions
    {
        public string sourceType { get; set; }
        public object defaultValue { get; set; }
        public string value { get; set; }
        public double? min { get; set; }
        public double? max { get; set; }
        public double? step { get; set; }
        public string formatString { get; set; }
        public string sourceForm { get; set; }
        public string scriptName { get; set; }
        public string propertyName { get; set; }
        public List<string> options { get; set; }
        public List<string> shortNames { get; set; }
    }

    public class GroupCondition
    {
        [JsonProperty("NOT")]
        public List<int> NOTParameters { get; set; }
        [JsonProperty("OR")]
        public List<int> ORParameters { get; set; }
        [JsonProperty("AND")]
        public List<int> ANDParameters { get; set; }
        [JsonProperty("ONLY")]
        public List<int> ONLYParameters { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public string form { get; set; }
        public string scriptName { get; set; }
        public string function { get; set; }
        [JsonProperty("params")]
        public List<object> parameters { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public string text { get; set; }
        public string id { get; set; }
        public string help { get; set; }
        public bool? ignoreConflicts { get; set; }
        public ValueOptions valueOptions { get; set; }
        public int? position { get; set; }
        public Action action { get; set; }
        public int? groupControl { get; set; }
        public string groupBehavior { get; set; }
        public object groupCondition { get; set; }  
    }

    public class Page
    {
        public string pageDisplayName { get; set; }
        public string cursorFillMode { get; set; }
        public List<Content> content { get; set; }
    }

    public class CustomContent
    {
        public string source { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
    }

    public class Root
    {
        public string modName { get; set; }
        public string displayName { get; set; }
        public int? minMcmVersion { get; set; }
        public List<string> pluginRequirements { get; set; }
        public CustomContent customContent { get; set; }
        public string cursorFillMode { get; set; }
        public List<Content> content { get; set; }
        public List<Page> pages { get; set; }
        
    }
}
