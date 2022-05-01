using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCMHelper_MenuDesigner
{
    // RootKeybind myDeserializedClass = JsonConvert.DeserializeObject<RootKeybind>(myJsonResponse);
    public class KeybindAction
    {
        public string type { get; set; }
        public string form { get; set; }
        public string scriptName { get; set; }
        public string function { get; set; }
        public string command { get; set; }
        [JsonProperty("params")]
        public List<object> parameters { get; set; }
    }

    public class Keybinds
    {
        public string id { get; set; }
        public string desc { get; set; }
        public KeybindAction action { get; set; }
    }
    public class RootKeybind
    {
        public string modName { get; set; }
        public List<Keybinds> keybinds { get; set; }
        
    }
}
